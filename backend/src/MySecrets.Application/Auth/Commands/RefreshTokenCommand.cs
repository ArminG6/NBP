using FluentValidation;
using MediatR;
using MySecrets.Application.Auth.DTOs;
using MySecrets.Application.Common.Interfaces;
using MySecrets.Application.Common.Models;
using MySecrets.Domain.Entities;
using MySecrets.Domain.Interfaces;

namespace MySecrets.Application.Auth.Commands;

public record RefreshTokenCommand(
    string RefreshToken,
    string IpAddress
) : IRequest<Result<AuthResponseDto>>;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required");
    }
}

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly IAuditLogService _auditLogService;

    public RefreshTokenCommandHandler(
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        IAuditLogService auditLogService)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _auditLogService = auditLogService;
    }

    public async Task<Result<AuthResponseDto>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        var tokenHash = _jwtService.HashToken(request.RefreshToken);
        var storedToken = await _unitOfWork.RefreshTokens.GetByTokenHashAsync(tokenHash, cancellationToken);

        if (storedToken == null)
        {
            return Result<AuthResponseDto>.Failure("Invalid refresh token");
        }

        if (!storedToken.IsActive)
        {
            // Token reuse detected - revoke all tokens for this user
            if (storedToken.IsRevoked)
            {
                await _unitOfWork.RefreshTokens.RevokeAllUserTokensAsync(
                    storedToken.UserId, 
                    "Attempted reuse of revoked token", 
                    request.IpAddress, 
                    cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                
                await _auditLogService.LogAuthEventAsync(
                    storedToken.UserId, "unknown", "RefreshToken", false, request.IpAddress, 
                    "Token reuse detected - all tokens revoked", cancellationToken);
            }
            
            return Result<AuthResponseDto>.Failure("Invalid refresh token");
        }

        var user = await _unitOfWork.Users.GetByIdAsync(storedToken.UserId, cancellationToken);
        
        if (user == null || !user.IsActive)
        {
            return Result<AuthResponseDto>.Failure("User not found or inactive");
        }

        // Revoke current token
        storedToken.IsRevoked = true;
        storedToken.RevokedAt = DateTime.UtcNow;
        storedToken.RevokedByIp = request.IpAddress;
        storedToken.RevokedReason = "Replaced by new token";

        // Generate new tokens
        var newAccessToken = _jwtService.GenerateAccessToken(user);
        var newRefreshToken = _jwtService.GenerateRefreshToken();
        var newRefreshTokenHash = _jwtService.HashToken(newRefreshToken);

        storedToken.ReplacedByTokenHash = newRefreshTokenHash;
        await _unitOfWork.RefreshTokens.UpdateAsync(storedToken, cancellationToken);

        // Save new refresh token
        var newRefreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = newRefreshTokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = request.IpAddress
        };

        await _unitOfWork.RefreshTokens.AddAsync(newRefreshTokenEntity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAuthEventAsync(
            user.Id, user.Email, "RefreshToken", true, request.IpAddress, null, cancellationToken);

        return Result<AuthResponseDto>.Success(new AuthResponseDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            newAccessToken,
            newRefreshToken,
            DateTime.UtcNow.AddMinutes(15)
        ));
    }
}

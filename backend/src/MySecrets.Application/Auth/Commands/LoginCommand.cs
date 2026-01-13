using FluentValidation;
using MediatR;
using MySecrets.Application.Auth.DTOs;
using MySecrets.Application.Common.Interfaces;
using MySecrets.Application.Common.Models;
using MySecrets.Domain.Entities;
using MySecrets.Domain.Interfaces;

namespace MySecrets.Application.Auth.Commands;

public record LoginCommand(
    string Email,
    string Password,
    string IpAddress
) : IRequest<Result<AuthResponseDto>>;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly IAuditLogService _auditLogService;

    public LoginCommandHandler(
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        IAuditLogService auditLogService)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _auditLogService = auditLogService;
    }

    public async Task<Result<AuthResponseDto>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(
            request.Email.ToLowerInvariant(), cancellationToken);

        if (user == null)
        {
            await _auditLogService.LogAuthEventAsync(
                null, request.Email, "Login", false, request.IpAddress, "User not found", cancellationToken);
            return Result<AuthResponseDto>.Failure("Invalid email or password");
        }

        if (user.IsGoogleUser)
        {
            await _auditLogService.LogAuthEventAsync(
                user.Id, request.Email, "Login", false, request.IpAddress, "Google user attempted password login", cancellationToken);
            return Result<AuthResponseDto>.Failure("This account uses Google login. Please sign in with Google.");
        }

        if (!user.IsActive)
        {
            await _auditLogService.LogAuthEventAsync(
                user.Id, request.Email, "Login", false, request.IpAddress, "Account inactive", cancellationToken);
            return Result<AuthResponseDto>.Failure("Account is inactive");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            await _auditLogService.LogAuthEventAsync(
                user.Id, request.Email, "Login", false, request.IpAddress, "Invalid password", cancellationToken);
            return Result<AuthResponseDto>.Failure("Invalid email or password");
        }

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _unitOfWork.Users.UpdateAsync(user, cancellationToken);

        // Generate tokens
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();
        var refreshTokenHash = _jwtService.HashToken(refreshToken);

        // Save refresh token
        var refreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = refreshTokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = request.IpAddress
        };

        await _unitOfWork.RefreshTokens.AddAsync(refreshTokenEntity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogAuthEventAsync(
            user.Id, user.Email, "Login", true, request.IpAddress, null, cancellationToken);

        return Result<AuthResponseDto>.Success(new AuthResponseDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            accessToken,
            refreshToken,
            DateTime.UtcNow.AddMinutes(15)
        ));
    }
}

using FluentValidation;
using MediatR;
using MySecrets.Application.Auth.DTOs;
using MySecrets.Application.Common.Interfaces;
using MySecrets.Application.Common.Models;
using MySecrets.Domain.Entities;
using MySecrets.Domain.Interfaces;

namespace MySecrets.Application.Auth.Commands;

public record GoogleLoginCommand(
    string IdToken,
    string IpAddress
) : IRequest<Result<AuthResponseDto>>;

public class GoogleLoginCommandValidator : AbstractValidator<GoogleLoginCommand>
{
    public GoogleLoginCommandValidator()
    {
        RuleFor(x => x.IdToken)
            .NotEmpty().WithMessage("Google ID token is required");
    }
}

public class GoogleLoginCommandHandler : IRequestHandler<GoogleLoginCommand, Result<AuthResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly IGoogleAuthService _googleAuthService;
    private readonly IAuditLogService _auditLogService;

    public GoogleLoginCommandHandler(
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        IGoogleAuthService googleAuthService,
        IAuditLogService auditLogService)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _googleAuthService = googleAuthService;
        _auditLogService = auditLogService;
    }

    public async Task<Result<AuthResponseDto>> Handle(
        GoogleLoginCommand request,
        CancellationToken cancellationToken)
    {
        // Validate Google token
        var googleUser = await _googleAuthService.ValidateTokenAsync(request.IdToken, cancellationToken);
        
        if (googleUser == null)
        {
            await _auditLogService.LogAuthEventAsync(
                null, "unknown", "GoogleLogin", false, request.IpAddress, "Invalid Google token", cancellationToken);
            return Result<AuthResponseDto>.Failure("Invalid Google token");
        }

        // Check if user exists by Google ID
        var user = await _unitOfWork.Users.GetByGoogleIdAsync(googleUser.GoogleId, cancellationToken);
        
        if (user == null)
        {
            // Check if email is already used by a non-Google account
            var existingUser = await _unitOfWork.Users.GetByEmailAsync(googleUser.Email.ToLowerInvariant(), cancellationToken);
            
            if (existingUser != null && !existingUser.IsGoogleUser)
            {
                await _auditLogService.LogAuthEventAsync(
                    existingUser.Id, googleUser.Email, "GoogleLogin", false, request.IpAddress, 
                    "Email already registered with password", cancellationToken);
                return Result<AuthResponseDto>.Failure(
                    "An account with this email already exists. Please login with your password.");
            }

            // Create new user
            user = new User
            {
                Id = Guid.NewGuid(),
                Email = googleUser.Email.ToLowerInvariant(),
                PasswordHash = string.Empty, // Google users don't have passwords
                FirstName = googleUser.FirstName,
                LastName = googleUser.LastName,
                IsGoogleUser = true,
                GoogleId = googleUser.GoogleId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
            };

            await _unitOfWork.Users.AddAsync(user, cancellationToken);
        }
        else
        {
            if (!user.IsActive)
            {
                await _auditLogService.LogAuthEventAsync(
                    user.Id, user.Email, "GoogleLogin", false, request.IpAddress, "Account inactive", cancellationToken);
                return Result<AuthResponseDto>.Failure("Account is inactive");
            }

            user.LastLoginAt = DateTime.UtcNow;
            await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
        }

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
            user.Id, user.Email, "GoogleLogin", true, request.IpAddress, null, cancellationToken);

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

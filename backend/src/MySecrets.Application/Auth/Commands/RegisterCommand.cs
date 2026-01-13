using FluentValidation;
using MediatR;
using MySecrets.Application.Auth.DTOs;
using MySecrets.Application.Common.Models;
using MySecrets.Domain.Entities;
using MySecrets.Domain.Interfaces;

namespace MySecrets.Application.Auth.Commands;

public record RegisterCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string IpAddress
) : IRequest<Result<AuthResponseDto>>;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .MaximumLength(128).WithMessage("Password must not exceed 128 characters")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches("[0-9]").WithMessage("Password must contain at least one number")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");
    }
}

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<AuthResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly Common.Interfaces.IJwtService _jwtService;
    private readonly IAuditLogService _auditLogService;

    public RegisterCommandHandler(
        IUnitOfWork unitOfWork,
        Common.Interfaces.IJwtService jwtService,
        IAuditLogService auditLogService)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _auditLogService = auditLogService;
    }

    public async Task<Result<AuthResponseDto>> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        // Check if email already exists
        if (await _unitOfWork.Users.EmailExistsAsync(request.Email, cancellationToken))
        {
            await _auditLogService.LogAuthEventAsync(
                null, request.Email, "Register", false, request.IpAddress, "Email already exists", cancellationToken);
            return Result<AuthResponseDto>.Failure("An account with this email already exists");
        }

        // Create user
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email.ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            IsGoogleUser = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Users.AddAsync(user, cancellationToken);

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
            user.Id, user.Email, "Register", true, request.IpAddress, null, cancellationToken);

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

using FluentValidation;
using MediatR;
using MySecrets.Application.Common.Interfaces;
using MySecrets.Application.Common.Models;
using MySecrets.Application.Secrets.DTOs;
using MySecrets.Domain.Interfaces;

namespace MySecrets.Application.Secrets.Commands;

public record UpdateSecretCommand(
    Guid Id,
    string WebsiteUrl,
    string Username,
    string? Password,
    string? Notes,
    string? Category,
    bool IsFavorite
) : IRequest<Result<SecretDto>>;

public class UpdateSecretCommandValidator : AbstractValidator<UpdateSecretCommand>
{
    public UpdateSecretCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Secret ID is required");

        RuleFor(x => x.WebsiteUrl)
            .NotEmpty().WithMessage("Website URL is required")
            .MaximumLength(2048).WithMessage("Website URL must not exceed 2048 characters");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .MaximumLength(256).WithMessage("Username must not exceed 256 characters");

        RuleFor(x => x.Password)
            .MaximumLength(1024).WithMessage("Password must not exceed 1024 characters")
            .When(x => !string.IsNullOrEmpty(x.Password));

        RuleFor(x => x.Notes)
            .MaximumLength(4000).WithMessage("Notes must not exceed 4000 characters");

        RuleFor(x => x.Category)
            .MaximumLength(100).WithMessage("Category must not exceed 100 characters");
    }
}

public class UpdateSecretCommandHandler : IRequestHandler<UpdateSecretCommand, Result<SecretDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEncryptionService _encryptionService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogService _auditLogService;

    public UpdateSecretCommandHandler(
        IUnitOfWork unitOfWork,
        IEncryptionService encryptionService,
        ICurrentUserService currentUserService,
        IAuditLogService auditLogService)
    {
        _unitOfWork = unitOfWork;
        _encryptionService = encryptionService;
        _currentUserService = currentUserService;
        _auditLogService = auditLogService;
    }

    public async Task<Result<SecretDto>> Handle(
        UpdateSecretCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Result<SecretDto>.Failure("User not authenticated");
        }

        var userId = _currentUserService.UserId.Value;

        var secret = await _unitOfWork.Secrets.GetByIdAndUserIdAsync(
            request.Id, userId, cancellationToken);

        if (secret == null)
        {
            return Result<SecretDto>.Failure("Secret not found");
        }

        secret.WebsiteUrl = request.WebsiteUrl;
        secret.Username = request.Username;
        secret.Notes = request.Notes;
        secret.Category = request.Category;
        secret.IsFavorite = request.IsFavorite;
        secret.UpdatedAt = DateTime.UtcNow;

        // Only update password if provided
        if (!string.IsNullOrEmpty(request.Password))
        {
            secret.EncryptedPassword = _encryptionService.Encrypt(request.Password, userId);
        }

        await _unitOfWork.Secrets.UpdateAsync(secret, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogSecretAccessAsync(
            userId, secret.Id, "Updated", null, cancellationToken);

        return Result<SecretDto>.Success(new SecretDto(
            secret.Id,
            secret.WebsiteUrl,
            secret.Username,
            secret.Notes,
            secret.Category,
            secret.IsFavorite,
            secret.CreatedAt,
            secret.UpdatedAt
        ));
    }
}

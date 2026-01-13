using FluentValidation;
using MediatR;
using MySecrets.Application.Common.Interfaces;
using MySecrets.Application.Common.Models;
using MySecrets.Application.Secrets.DTOs;
using MySecrets.Domain.Entities;
using MySecrets.Domain.Interfaces;

namespace MySecrets.Application.Secrets.Commands;

public record CreateSecretCommand(
    string WebsiteUrl,
    string Username,
    string Password,
    string? Notes,
    string? Category,
    bool IsFavorite
) : IRequest<Result<SecretDto>>;

public class CreateSecretCommandValidator : AbstractValidator<CreateSecretCommand>
{
    public CreateSecretCommandValidator()
    {
        RuleFor(x => x.WebsiteUrl)
            .NotEmpty().WithMessage("Website URL is required")
            .MaximumLength(2048).WithMessage("Website URL must not exceed 2048 characters");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .MaximumLength(256).WithMessage("Username must not exceed 256 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MaximumLength(1024).WithMessage("Password must not exceed 1024 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(4000).WithMessage("Notes must not exceed 4000 characters");

        RuleFor(x => x.Category)
            .MaximumLength(100).WithMessage("Category must not exceed 100 characters");
    }
}

public class CreateSecretCommandHandler : IRequestHandler<CreateSecretCommand, Result<SecretDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEncryptionService _encryptionService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogService _auditLogService;

    public CreateSecretCommandHandler(
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
        CreateSecretCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Result<SecretDto>.Failure("User not authenticated");
        }

        var userId = _currentUserService.UserId.Value;

        // Encrypt password
        var encryptedPassword = _encryptionService.Encrypt(request.Password, userId);

        var secret = new Secret
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            WebsiteUrl = request.WebsiteUrl,
            Username = request.Username,
            EncryptedPassword = encryptedPassword,
            Notes = request.Notes,
            Category = request.Category,
            IsFavorite = request.IsFavorite,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Secrets.AddAsync(secret, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogSecretAccessAsync(
            userId, secret.Id, "Created", null, cancellationToken);

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

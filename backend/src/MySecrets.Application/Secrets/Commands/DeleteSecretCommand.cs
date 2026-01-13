using FluentValidation;
using MediatR;
using MySecrets.Application.Common.Interfaces;
using MySecrets.Application.Common.Models;
using MySecrets.Domain.Interfaces;

namespace MySecrets.Application.Secrets.Commands;

public record DeleteSecretCommand(Guid Id) : IRequest<Result>;

public class DeleteSecretCommandValidator : AbstractValidator<DeleteSecretCommand>
{
    public DeleteSecretCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Secret ID is required");
    }
}

public class DeleteSecretCommandHandler : IRequestHandler<DeleteSecretCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogService _auditLogService;

    public DeleteSecretCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IAuditLogService auditLogService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _auditLogService = auditLogService;
    }

    public async Task<Result> Handle(DeleteSecretCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Result.Failure("User not authenticated");
        }

        var userId = _currentUserService.UserId.Value;

        var secret = await _unitOfWork.Secrets.GetByIdAndUserIdAsync(
            request.Id, userId, cancellationToken);

        if (secret == null)
        {
            return Result.Failure("Secret not found");
        }

        await _unitOfWork.Secrets.DeleteAsync(secret, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogService.LogSecretAccessAsync(
            userId, request.Id, "Deleted", null, cancellationToken);

        return Result.Success();
    }
}

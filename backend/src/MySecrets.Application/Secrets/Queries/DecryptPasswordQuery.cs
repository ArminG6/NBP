using MediatR;
using MySecrets.Application.Common.Interfaces;
using MySecrets.Application.Common.Models;
using MySecrets.Domain.Interfaces;

namespace MySecrets.Application.Secrets.Queries;

public record DecryptPasswordQuery(Guid SecretId) : IRequest<Result<string>>;

public class DecryptPasswordQueryHandler : IRequestHandler<DecryptPasswordQuery, Result<string>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEncryptionService _encryptionService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogService _auditLogService;

    public DecryptPasswordQueryHandler(
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

    public async Task<Result<string>> Handle(
        DecryptPasswordQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Result<string>.Failure("User not authenticated");
        }

        var userId = _currentUserService.UserId.Value;

        var secret = await _unitOfWork.Secrets.GetByIdAndUserIdAsync(
            request.SecretId, userId, cancellationToken);

        if (secret == null)
        {
            return Result<string>.Failure("Secret not found");
        }

        try
        {
            var decryptedPassword = _encryptionService.Decrypt(secret.EncryptedPassword, userId);

            // Log password access
            await _auditLogService.LogSecretAccessAsync(
                userId, secret.Id, "PasswordDecrypted", null, cancellationToken);

            return Result<string>.Success(decryptedPassword);
        }
        catch (Exception)
        {
            return Result<string>.Failure("Failed to decrypt password");
        }
    }
}

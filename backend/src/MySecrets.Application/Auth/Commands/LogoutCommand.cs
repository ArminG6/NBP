using MediatR;
using MySecrets.Application.Common.Interfaces;
using MySecrets.Application.Common.Models;
using MySecrets.Domain.Interfaces;

namespace MySecrets.Application.Auth.Commands;

public record LogoutCommand(
    string RefreshToken,
    string IpAddress
) : IRequest<Result>;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly IAuditLogService _auditLogService;

    public LogoutCommandHandler(
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        IAuditLogService auditLogService)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _auditLogService = auditLogService;
    }

    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.RefreshToken))
        {
            return Result.Success();
        }

        var tokenHash = _jwtService.HashToken(request.RefreshToken);
        var storedToken = await _unitOfWork.RefreshTokens.GetByTokenHashAsync(tokenHash, cancellationToken);

        if (storedToken != null && storedToken.IsActive)
        {
            storedToken.IsRevoked = true;
            storedToken.RevokedAt = DateTime.UtcNow;
            storedToken.RevokedByIp = request.IpAddress;
            storedToken.RevokedReason = "Logged out";

            await _unitOfWork.RefreshTokens.UpdateAsync(storedToken, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _auditLogService.LogAuthEventAsync(
                storedToken.UserId, "unknown", "Logout", true, request.IpAddress, null, cancellationToken);
        }

        return Result.Success();
    }
}

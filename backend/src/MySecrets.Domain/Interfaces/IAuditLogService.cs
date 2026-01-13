namespace MySecrets.Domain.Interfaces;

/// <summary>
/// Service for logging audit events to MongoDB.
/// </summary>
public interface IAuditLogService
{
    Task LogAsync(string action, Guid userId, string details, string? ipAddress = null, CancellationToken cancellationToken = default);
    Task LogSecretAccessAsync(Guid userId, Guid secretId, string action, string? ipAddress = null, CancellationToken cancellationToken = default);
    Task LogAuthEventAsync(Guid? userId, string email, string action, bool success, string? ipAddress = null, string? reason = null, CancellationToken cancellationToken = default);
    Task LogExportAsync(Guid userId, string format, int secretCount, string? ipAddress = null, CancellationToken cancellationToken = default);
}

using Microsoft.Extensions.Logging;
using MySecrets.Domain.Interfaces;
using MySecrets.Infrastructure.MongoDB;
using MySecrets.Infrastructure.MongoDB.Documents;

namespace MySecrets.Infrastructure.Services;

public class AuditLogService : IAuditLogService
{
    private readonly MongoDbContext _mongoContext;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(MongoDbContext mongoContext, ILogger<AuditLogService> logger)
    {
        _mongoContext = mongoContext;
        _logger = logger;
    }

    public async Task LogAsync(
        string action,
        Guid userId,
        string details,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var log = new AuditLog
            {
                UserId = userId,
                Action = action,
                Details = details,
                IpAddress = ipAddress,
                Timestamp = DateTime.UtcNow,
                Category = "General"
            };

            await _mongoContext.AuditLogs.InsertOneAsync(log, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            // Log but don't throw - audit logging should not break main flow
            _logger.LogError(ex, "Failed to write audit log for action {Action}", action);
        }
    }

    public async Task LogSecretAccessAsync(
        Guid userId,
        Guid secretId,
        string action,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var log = new SecretAccessLog
            {
                UserId = userId,
                SecretId = secretId,
                Action = action,
                Details = $"Secret {secretId} - {action}",
                IpAddress = ipAddress,
                Timestamp = DateTime.UtcNow,
                Category = "SecretAccess"
            };

            await _mongoContext.SecretAccessLogs.InsertOneAsync(log, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write secret access log for secret {SecretId}", secretId);
        }
    }

    public async Task LogAuthEventAsync(
        Guid? userId,
        string email,
        string action,
        bool success,
        string? ipAddress = null,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var log = new AuthEventLog
            {
                UserId = userId,
                Email = email,
                Action = action,
                Success = success,
                Reason = reason,
                Details = $"Auth event: {action} for {email} - {(success ? "Success" : "Failed")}",
                IpAddress = ipAddress,
                Timestamp = DateTime.UtcNow,
                Category = "Authentication"
            };

            await _mongoContext.AuthEventLogs.InsertOneAsync(log, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write auth event log for {Email}", email);
        }
    }

    public async Task LogExportAsync(
        Guid userId,
        string format,
        int secretCount,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var log = new ExportLog
            {
                UserId = userId,
                Format = format,
                SecretCount = secretCount,
                Action = "Export",
                Details = $"Exported {secretCount} secrets in {format} format",
                IpAddress = ipAddress,
                Timestamp = DateTime.UtcNow,
                Category = "Export"
            };

            await _mongoContext.ExportLogs.InsertOneAsync(log, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write export log for user {UserId}", userId);
        }
    }
}

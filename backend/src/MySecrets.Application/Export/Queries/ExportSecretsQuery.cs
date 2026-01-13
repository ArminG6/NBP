using System.Text;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MySecrets.Application.Common.Interfaces;
using MySecrets.Application.Common.Models;
using MySecrets.Application.Export.DTOs;
using MySecrets.Domain.Interfaces;

namespace MySecrets.Application.Export.Queries;

public record ExportSecretsQuery(ExportFormat Format) : IRequest<Result<ExportResultDto>>;

public class ExportSecretsQueryHandler : IRequestHandler<ExportSecretsQuery, Result<ExportResultDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogService _auditLogService;

    public ExportSecretsQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IAuditLogService auditLogService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _auditLogService = auditLogService;
    }

    public async Task<Result<ExportResultDto>> Handle(
        ExportSecretsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
        {
            return Result<ExportResultDto>.Failure("User not authenticated");
        }

        var userId = _currentUserService.UserId.Value;

        var secrets = await _unitOfWork.Secrets
            .GetQueryableByUserId(userId)
            .OrderBy(s => s.WebsiteUrl)
            .Select(s => new ExportSecretRowDto(
                s.WebsiteUrl,
                s.Username,
                s.EncryptedPassword, // Password stays ENCRYPTED
                s.Notes,
                s.Category
            ))
            .ToListAsync(cancellationToken);

        var (content, contentType, fileName) = request.Format switch
        {
            ExportFormat.Csv => GenerateCsv(secrets),
            ExportFormat.Txt => GenerateTxt(secrets),
            _ => throw new ArgumentOutOfRangeException()
        };

        await _auditLogService.LogExportAsync(
            userId, request.Format.ToString(), secrets.Count, null, cancellationToken);

        return Result<ExportResultDto>.Success(new ExportResultDto(
            content,
            contentType,
            fileName
        ));
    }

    private static (byte[] Content, string ContentType, string FileName) GenerateCsv(
        List<ExportSecretRowDto> secrets)
    {
        var sb = new StringBuilder();
        
        // Header
        sb.AppendLine("Website URL,Username,Encrypted Password,Notes,Category");

        // Data rows
        foreach (var secret in secrets)
        {
            sb.AppendLine($"\"{EscapeCsv(secret.WebsiteUrl)}\",\"{EscapeCsv(secret.Username)}\",\"{EscapeCsv(secret.EncryptedPassword)}\",\"{EscapeCsv(secret.Notes ?? "")}\",\"{EscapeCsv(secret.Category ?? "")}\"");
        }

        var content = Encoding.UTF8.GetBytes(sb.ToString());
        var fileName = $"my_secrets_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";

        return (content, "text/csv", fileName);
    }

    private static (byte[] Content, string ContentType, string FileName) GenerateTxt(
        List<ExportSecretRowDto> secrets)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("═══════════════════════════════════════════════════════════════");
        sb.AppendLine("                     MY SECRETS EXPORT");
        sb.AppendLine($"                   {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        sb.AppendLine("═══════════════════════════════════════════════════════════════");
        sb.AppendLine();
        sb.AppendLine("NOTE: Passwords are ENCRYPTED. Do NOT share this file.");
        sb.AppendLine();

        var index = 1;
        foreach (var secret in secrets)
        {
            sb.AppendLine($"───────────────────────────────────────────────────────────────");
            sb.AppendLine($"[{index}] {secret.WebsiteUrl}");
            sb.AppendLine($"───────────────────────────────────────────────────────────────");
            sb.AppendLine($"  Username: {secret.Username}");
            sb.AppendLine($"  Password (encrypted): {secret.EncryptedPassword}");
            
            if (!string.IsNullOrEmpty(secret.Category))
                sb.AppendLine($"  Category: {secret.Category}");
            
            if (!string.IsNullOrEmpty(secret.Notes))
                sb.AppendLine($"  Notes: {secret.Notes}");
            
            sb.AppendLine();
            index++;
        }

        sb.AppendLine("═══════════════════════════════════════════════════════════════");
        sb.AppendLine($"                   Total: {secrets.Count} secrets");
        sb.AppendLine("═══════════════════════════════════════════════════════════════");

        var content = Encoding.UTF8.GetBytes(sb.ToString());
        var fileName = $"my_secrets_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.txt";

        return (content, "text/plain", fileName);
    }

    private static string EscapeCsv(string value)
    {
        return value.Replace("\"", "\"\"");
    }
}

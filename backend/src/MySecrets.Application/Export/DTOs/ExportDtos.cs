namespace MySecrets.Application.Export.DTOs;

public enum ExportFormat
{
    Csv,
    Txt
}

public record ExportResultDto(
    byte[] Content,
    string ContentType,
    string FileName
);

public record ExportSecretRowDto(
    string WebsiteUrl,
    string Username,
    string EncryptedPassword,
    string? Notes,
    string? Category
);

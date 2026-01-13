namespace MySecrets.Application.Secrets.DTOs;

public record SecretDto(
    Guid Id,
    string WebsiteUrl,
    string Username,
    string? Notes,
    string? Category,
    bool IsFavorite,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record SecretDetailDto(
    Guid Id,
    string WebsiteUrl,
    string Username,
    string? Notes,
    string? Category,
    bool IsFavorite,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record SecretWithPasswordDto(
    Guid Id,
    string WebsiteUrl,
    string Username,
    string DecryptedPassword,
    string? Notes,
    string? Category,
    bool IsFavorite
);

public record CreateSecretDto(
    string WebsiteUrl,
    string Username,
    string Password,
    string? Notes,
    string? Category,
    bool IsFavorite = false
);

public record UpdateSecretDto(
    string WebsiteUrl,
    string Username,
    string? Password, // Optional - only update if provided
    string? Notes,
    string? Category,
    bool IsFavorite
);

public record SecretListResponseDto(
    List<SecretDto> Items,
    int PageNumber,
    int PageSize,
    int TotalPages,
    int TotalCount,
    bool HasPreviousPage,
    bool HasNextPage
);

namespace MySecrets.Application.Auth.DTOs;

public record RegisterDto(
    string Email,
    string Password,
    string FirstName,
    string LastName
);

public record LoginDto(
    string Email,
    string Password
);

public record GoogleLoginDto(
    string IdToken
);

public record RefreshTokenDto(
    string RefreshToken
);

public record AuthResponseDto(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiry
);

public record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    bool IsGoogleUser,
    DateTime CreatedAt
);

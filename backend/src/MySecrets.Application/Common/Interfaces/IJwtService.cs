using MySecrets.Domain.Entities;

namespace MySecrets.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    string HashToken(string token);
    bool ValidateRefreshTokenHash(string token, string hash);
}

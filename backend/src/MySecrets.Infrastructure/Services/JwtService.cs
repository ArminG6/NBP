using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MySecrets.Application.Common.Interfaces;
using MySecrets.Domain.Entities;

namespace MySecrets.Infrastructure.Services;

public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessTokenExpirationMinutes { get; set; } = 15;
    public int RefreshTokenExpirationDays { get; set; } = 7;
}

public class JwtService : IJwtService
{
    private readonly JwtSettings _settings;
    private readonly SymmetricSecurityKey _securityKey;

    public JwtService(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;

        if (string.IsNullOrEmpty(_settings.SecretKey) || _settings.SecretKey.Length < 32)
        {
            throw new InvalidOperationException(
                "JWT secret key must be at least 32 characters. Set JWT_SECRET_KEY environment variable.");
        }

        _securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
    }

    public string GenerateAccessToken(User user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("firstName", user.FirstName),
            new("lastName", user.LastName),
            new("isGoogleUser", user.IsGoogleUser.ToString().ToLower())
        };

        var credentials = new SigningCredentials(_securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }

    public string HashToken(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }

    public bool ValidateRefreshTokenHash(string token, string hash)
    {
        var computedHash = HashToken(token);
        return computedHash == hash;
    }
}

using Google.Apis.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySecrets.Application.Common.Interfaces;

namespace MySecrets.Infrastructure.Services;

public class GoogleAuthSettings
{
    public string ClientId { get; set; } = string.Empty;
}

public class GoogleAuthService : IGoogleAuthService
{
    private readonly GoogleAuthSettings _settings;
    private readonly ILogger<GoogleAuthService> _logger;

    public GoogleAuthService(
        IOptions<GoogleAuthSettings> settings,
        ILogger<GoogleAuthService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<GoogleUserInfo?> ValidateTokenAsync(
        string idToken,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var validationSettings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _settings.ClientId }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, validationSettings);

            return new GoogleUserInfo
            {
                GoogleId = payload.Subject,
                Email = payload.Email,
                FirstName = payload.GivenName ?? payload.Name?.Split(' ').FirstOrDefault() ?? "",
                LastName = payload.FamilyName ?? payload.Name?.Split(' ').Skip(1).FirstOrDefault() ?? "",
                PictureUrl = payload.Picture
            };
        }
        catch (InvalidJwtException ex)
        {
            _logger.LogWarning(ex, "Invalid Google JWT token");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Google token");
            return null;
        }
    }
}

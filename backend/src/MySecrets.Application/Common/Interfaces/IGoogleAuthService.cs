namespace MySecrets.Application.Common.Interfaces;

public class GoogleUserInfo
{
    public string GoogleId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PictureUrl { get; set; }
}

public interface IGoogleAuthService
{
    Task<GoogleUserInfo?> ValidateTokenAsync(string idToken, CancellationToken cancellationToken = default);
}

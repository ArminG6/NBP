using MySecrets.Domain.Entities;

namespace MySecrets.Domain.Interfaces;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task RevokeAllUserTokensAsync(Guid userId, string reason, string? ipAddress = null, CancellationToken cancellationToken = default);
}

using Microsoft.EntityFrameworkCore;
using MySecrets.Domain.Entities;
using MySecrets.Domain.Interfaces;

namespace MySecrets.Infrastructure.Persistence.Repositories;

public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<RefreshToken?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(
            rt => rt.TokenHash == tokenHash,
            cancellationToken);
    }

    public async Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }

    public async Task RevokeAllUserTokensAsync(
        Guid userId,
        string reason,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        var activeTokens = await GetActiveTokensByUserIdAsync(userId, cancellationToken);

        foreach (var token in activeTokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedByIp = ipAddress;
            token.RevokedReason = reason;
        }
    }
}

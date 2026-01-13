using Microsoft.EntityFrameworkCore;
using MySecrets.Domain.Entities;
using MySecrets.Domain.Interfaces;

namespace MySecrets.Infrastructure.Persistence.Repositories;

public class SecretRepository : Repository<Secret>, ISecretRepository
{
    public SecretRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Secret>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Secret?> GetByIdAndUserIdAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(
            s => s.Id == id && s.UserId == userId,
            cancellationToken);
    }

    public IQueryable<Secret> GetQueryableByUserId(Guid userId)
    {
        return _dbSet.Where(s => s.UserId == userId);
    }
}

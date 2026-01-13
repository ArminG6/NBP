using Microsoft.EntityFrameworkCore;
using MySecrets.Domain.Entities;
using MySecrets.Domain.Interfaces;

namespace MySecrets.Infrastructure.Persistence.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(
            u => u.Email == email.ToLowerInvariant(),
            cancellationToken);
    }

    public async Task<User?> GetByGoogleIdAsync(string googleId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(
            u => u.GoogleId == googleId,
            cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(
            u => u.Email == email.ToLowerInvariant(),
            cancellationToken);
    }
}

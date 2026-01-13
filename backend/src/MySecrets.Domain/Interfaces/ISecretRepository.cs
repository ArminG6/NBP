using MySecrets.Domain.Entities;

namespace MySecrets.Domain.Interfaces;

public interface ISecretRepository : IRepository<Secret>
{
    Task<IEnumerable<Secret>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Secret?> GetByIdAndUserIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    IQueryable<Secret> GetQueryableByUserId(Guid userId);
}

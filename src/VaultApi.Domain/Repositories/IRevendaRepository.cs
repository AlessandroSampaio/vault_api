using VaultApi.Domain.Entities;

namespace VaultApi.Domain.Repositories;

public interface IRevendaRepository
{
    Task AddAsync(Revenda revenda);
    Task<Revenda?> GetAsync(Guid id);
    Task<List<Revenda>> ListAsync();
    Task SaveChangesAsync();
}

using VaultApi.Domain.Entities;

namespace VaultApi.Domain.Repositories;

public interface IClienteRepository
{
    Task AddAsync(Cliente cliente);
    Task<Cliente?> GetAsync(Guid id);
    Task<List<Cliente>> ListAllAsync();
    Task<List<Cliente>> ListAsync(bool semRestricao, Guid? revendaId);
    Task SaveChangesAsync();
}

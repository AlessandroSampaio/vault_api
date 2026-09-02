using VaultApi.Domain.Entities;

namespace VaultApi.Domain.Repositories;

public interface IProdutoRepository
{
    Task AddAsync(Produto produto);
    Task AddModuloAsync(Modulo modulo);
    Task<Produto?> GetAsync(Guid id);
    Task<List<Produto>> ListAsync();
    Task SaveChangesAsync();
}

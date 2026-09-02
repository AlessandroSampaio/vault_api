using VaultApi.Domain.Entities;

namespace VaultApi.Domain.Repositories;

public interface IProdutoRepository
{
    Task AddAsync(Produto produto);
    Task<Produto?> GetAsync(Guid id);
    Task<List<Produto>> ListAsync();
    Task SaveChangesAsync();
}

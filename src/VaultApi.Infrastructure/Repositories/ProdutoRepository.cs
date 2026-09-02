using Microsoft.EntityFrameworkCore;
using VaultApi.Domain.Entities;
using VaultApi.Domain.Repositories;
using VaultApi.Infrastructure.Persistence;

namespace VaultApi.Infrastructure.Repositories;

public class ProdutoRepository(AppDbContext db) : IProdutoRepository
{
    public async Task AddAsync(Produto produto) => await db.Set<Produto>().AddAsync(produto);

    public async Task AddModuloAsync(Modulo modulo) => await db.Set<Modulo>().AddAsync(modulo);

    public Task<Produto?> GetAsync(Guid id) => db.Set<Produto>()
        .Include(p => p.PrecosPorUnidade)
        .Include(p => p.Modulos).ThenInclude(m => m.Variantes)
        .SingleOrDefaultAsync(p => p.Id == id);

    public Task<List<Produto>> ListAsync() => db.Set<Produto>()
        .Include(p => p.PrecosPorUnidade)
        .Include(p => p.Modulos).ThenInclude(m => m.Variantes)
        .OrderBy(p => p.Nome)
        .ToListAsync();

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}

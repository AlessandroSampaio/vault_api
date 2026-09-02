using VaultApi.Domain.Entities;
using VaultApi.Domain.Repositories;
using VaultApi.Infrastructure.Persistence;

namespace VaultApi.Infrastructure.Repositories;

public class HistoricoPrecoCatalogoRepository(AppDbContext db) : IHistoricoPrecoCatalogoRepository
{
    public async Task AddAsync(HistoricoPrecoCatalogo registro) => await db.Set<HistoricoPrecoCatalogo>().AddAsync(registro);
}

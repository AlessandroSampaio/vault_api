using Microsoft.EntityFrameworkCore;
using VaultApi.Domain.Entities;
using VaultApi.Domain.Repositories;
using VaultApi.Infrastructure.Persistence;

namespace VaultApi.Infrastructure.Repositories;

public class ContratoRepository(AppDbContext db) : IContratoRepository
{
    public async Task AddAsync(Contrato contrato) => await db.Set<Contrato>().AddAsync(contrato);

    public Task<Contrato?> GetAsync(Guid id) => Query().SingleOrDefaultAsync(c => c.Id == id);

    public Task<List<Contrato>> ListAsync(bool semRestricao, Guid? revendaId) => Query()
        .Where(c => semRestricao || c.RevendaId == revendaId)
        .ToListAsync();

    public Task SaveChangesAsync() => db.SaveChangesAsync();

    private IQueryable<Contrato> Query() => db.Set<Contrato>()
        .Include(c => c.Itens).ThenInclude(i => i.Unidades)
        .Include(c => c.Itens).ThenInclude(i => i.Modulos);
}

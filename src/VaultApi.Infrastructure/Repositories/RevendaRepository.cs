using Microsoft.EntityFrameworkCore;
using VaultApi.Domain.Entities;
using VaultApi.Domain.Repositories;
using VaultApi.Infrastructure.Persistence;

namespace VaultApi.Infrastructure.Repositories;

public class RevendaRepository(AppDbContext db) : IRevendaRepository
{
    public async Task AddAsync(Revenda revenda) => await db.Set<Revenda>().AddAsync(revenda);
    public Task<Revenda?> GetAsync(Guid id) => db.Set<Revenda>().SingleOrDefaultAsync(r => r.Id == id);
    public Task<List<Revenda>> ListAsync() => db.Set<Revenda>().OrderBy(r => r.Nome).ToListAsync();
    public Task SaveChangesAsync() => db.SaveChangesAsync();
}

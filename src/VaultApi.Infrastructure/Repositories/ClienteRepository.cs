using Microsoft.EntityFrameworkCore;
using VaultApi.Domain.Entities;
using VaultApi.Domain.Repositories;
using VaultApi.Infrastructure.Persistence;

namespace VaultApi.Infrastructure.Repositories;

public class ClienteRepository(AppDbContext db) : IClienteRepository
{
    public async Task AddAsync(Cliente cliente) => await db.Set<Cliente>().AddAsync(cliente);
    public Task<Cliente?> GetAsync(Guid id) => db.Set<Cliente>().SingleOrDefaultAsync(c => c.Id == id);
    public Task<List<Cliente>> ListAllAsync() => db.Set<Cliente>().OrderBy(c => c.RazaoSocial).ToListAsync();
    public Task<List<Cliente>> ListAsync(bool semRestricao, Guid? revendaId) => db.Set<Cliente>()
        .Where(c => semRestricao || c.RevendaId == revendaId)
        .OrderBy(c => c.RazaoSocial)
        .ToListAsync();
    public Task SaveChangesAsync() => db.SaveChangesAsync();
}

using Microsoft.EntityFrameworkCore;
using VaultApi.Domain.Entities;
using VaultApi.Domain.Repositories;
using VaultApi.Infrastructure.Persistence;

namespace VaultApi.Infrastructure.Repositories;

public class LicencaRepository(AppDbContext db) : ILicencaRepository
{
    public async Task AddAsync(Licenca licenca) => await db.Set<Licenca>().AddAsync(licenca);
    public Task<List<Licenca>> ListarPorItemAsync(Guid contratoItemId) => db.Set<Licenca>()
        .Where(l => l.ContratoItemId == contratoItemId)
        .ToListAsync();
}

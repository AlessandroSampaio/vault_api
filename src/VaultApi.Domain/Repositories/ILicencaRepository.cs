using VaultApi.Domain.Entities;

namespace VaultApi.Domain.Repositories;

public interface ILicencaRepository
{
    Task AddAsync(Licenca licenca);
    Task<List<Licenca>> ListarPorItemAsync(Guid contratoItemId);
}

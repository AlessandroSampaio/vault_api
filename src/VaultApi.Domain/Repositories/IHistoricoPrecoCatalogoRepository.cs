using VaultApi.Domain.Entities;

namespace VaultApi.Domain.Repositories;

public interface IHistoricoPrecoCatalogoRepository
{
    Task AddAsync(HistoricoPrecoCatalogo registro);
}

using VaultApi.Domain.Entities;

namespace VaultApi.Domain.Repositories;

public interface IContratoRepository
{
    Task AddAsync(Contrato contrato);
    Task<Contrato?> GetAsync(Guid id);
    Task<List<Contrato>> ListAsync(bool semRestricao, Guid? revendaId);
    Task SaveChangesAsync();
}

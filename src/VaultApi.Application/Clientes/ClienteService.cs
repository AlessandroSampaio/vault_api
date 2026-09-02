using VaultApi.Domain.Entities;
using VaultApi.Domain.Repositories;

namespace VaultApi.Application.Clientes;

public class ClienteService(IClienteRepository repository)
{
    public async Task<ClienteResponse> CriarAsync(CriarClienteRequest request)
    {
        var cliente = new Cliente { Id = Guid.NewGuid(), Nome = request.Nome, Cnpj = request.Cnpj, RevendaId = request.RevendaId };
        await repository.AddAsync(cliente);
        await repository.SaveChangesAsync();
        return new ClienteResponse(cliente.Id, cliente.Nome, cliente.Cnpj, cliente.RevendaId);
    }
}

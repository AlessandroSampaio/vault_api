using VaultApi.Application.Abstractions;
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

    public async Task<List<ClienteResponse>> ListarAsync(ScopeResult scope)
    {
        var (semRestricao, revendaId) = scope switch
        {
            ScopeResult.SemRestricao => (true, (Guid?)null),
            ScopeResult.RestritoARevenda r => (false, r.RevendaId),
            _ => throw new InvalidOperationException()
        };

        var clientes = await repository.ListAsync(semRestricao, revendaId);
        return clientes.Select(c => new ClienteResponse(c.Id, c.Nome, c.Cnpj, c.RevendaId)).ToList();
    }
}

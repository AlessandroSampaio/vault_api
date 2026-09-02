using VaultApi.Application.Abstractions;
using VaultApi.Domain.Entities;
using VaultApi.Domain.Repositories;

namespace VaultApi.Application.Clientes;

public class ClienteService(IClienteRepository repository)
{
    public async Task<ClienteResponse> CriarAsync(CriarClienteRequest request)
    {
        var cliente = new Cliente
        {
            Id = Guid.NewGuid(),
            RazaoSocial = request.RazaoSocial,
            NomeFantasia = request.NomeFantasia,
            Cnpj = request.Cnpj,
            RevendaId = request.RevendaId,
            Cep = request.Cep,
            Logradouro = request.Logradouro,
            Numero = request.Numero,
            Complemento = request.Complemento,
            Bairro = request.Bairro,
            Cidade = request.Cidade,
            Estado = request.Estado,
            Email = request.Email,
            Telefone = request.Telefone,
            Whatsapp = request.Whatsapp,
            Responsavel = request.Responsavel
        };
        await repository.AddAsync(cliente);
        await repository.SaveChangesAsync();
        return ToResponse(cliente);
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
        return clientes.Select(ToResponse).ToList();
    }

    private static ClienteResponse ToResponse(Cliente c) => new(
        c.Id, c.RazaoSocial, c.NomeFantasia, c.Cnpj, c.RevendaId,
        c.Cep, c.Logradouro, c.Numero, c.Complemento, c.Bairro, c.Cidade, c.Estado,
        c.Email, c.Telefone, c.Whatsapp, c.Responsavel, c.CriadoEm);
}

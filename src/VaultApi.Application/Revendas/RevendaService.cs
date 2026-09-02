using VaultApi.Domain.Entities;
using VaultApi.Domain.Repositories;

namespace VaultApi.Application.Revendas;

public class RevendaService(IRevendaRepository repository)
{
    public async Task<RevendaResponse> CriarAsync(CriarRevendaRequest request)
    {
        var revenda = new Revenda { Id = Guid.NewGuid(), Nome = request.Nome, Cnpj = request.Cnpj, Ativo = true };
        await repository.AddAsync(revenda);
        await repository.SaveChangesAsync();
        return new RevendaResponse(revenda.Id, revenda.Nome, revenda.Cnpj, revenda.Ativo);
    }

    public async Task<List<RevendaResponse>> ListarAsync()
    {
        var revendas = await repository.ListAsync();
        return revendas.Select(r => new RevendaResponse(r.Id, r.Nome, r.Cnpj, r.Ativo)).ToList();
    }
}

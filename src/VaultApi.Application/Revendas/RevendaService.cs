using VaultApi.Domain.Entities;
using VaultApi.Domain.Repositories;

namespace VaultApi.Application.Revendas;

public class RevendaService(IRevendaRepository repository)
{
    public async Task<RevendaResponse> CriarAsync(CriarRevendaRequest request)
    {
        var revenda = new Revenda
        {
            Id = Guid.NewGuid(),
            RazaoSocial = request.RazaoSocial,
            NomeFantasia = request.NomeFantasia,
            Cnpj = request.Cnpj,
            Ativo = true,
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
        await repository.AddAsync(revenda);
        await repository.SaveChangesAsync();
        return ToResponse(revenda);
    }

    public async Task<List<RevendaResponse>> ListarAsync()
    {
        var revendas = await repository.ListAsync();
        return revendas.Select(ToResponse).ToList();
    }

    private static RevendaResponse ToResponse(Revenda r) => new(
        r.Id, r.RazaoSocial, r.NomeFantasia, r.Cnpj, r.Ativo,
        r.Cep, r.Logradouro, r.Numero, r.Complemento, r.Bairro, r.Cidade, r.Estado,
        r.Email, r.Telefone, r.Whatsapp, r.Responsavel, r.CriadoEm);
}

using System.Text;
using VaultApi.Domain.Entities;
using VaultApi.Domain.Enums;
using VaultApi.Domain.Repositories;

namespace VaultApi.Application.Licencas;

public class LicencaService(ILicencaRepository repository)
{
    private const string AlgoritmoPlaceholder = "PLACEHOLDER-v1";

    public async Task<Licenca> EmitirNovaVersaoAsync(ContratoItem item)
    {
        var ativas = (await repository.ListarPorItemAsync(item.Id)).Where(l => l.Status == StatusLicenca.Ativa);
        foreach (var ativa in ativas)
        {
            ativa.Status = StatusLicenca.Revogada;
        }

        var licenca = new Licenca
        {
            Id = Guid.NewGuid(),
            ContratoItemId = item.Id,
            Serial = ConstruirSerialOpaco(item),
            Algoritmo = AlgoritmoPlaceholder,
            DataEmissao = DateTimeOffset.UtcNow,
            Status = StatusLicenca.Ativa
        };

        await repository.AddAsync(licenca);
        return licenca;
    }

    private static string ConstruirSerialOpaco(ContratoItem item)
    {
        var conteudo = new StringBuilder()
            .Append(item.ProdutoId).Append('|')
            .Append(string.Join(',', item.Unidades.Select(u => $"{u.TipoUnidade}:{u.Quantidade}")))
            .Append('|')
            .Append(string.Join(',', item.Modulos.Where(m => m.Ativo).Select(m => m.ModuloId)));

        return Convert.ToBase64String(Encoding.UTF8.GetBytes(conteudo.ToString()));
    }
}

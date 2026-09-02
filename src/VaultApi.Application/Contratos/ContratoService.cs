using VaultApi.Application.Abstractions;
using VaultApi.Application.Pricing;
using VaultApi.Domain.Entities;
using VaultApi.Domain.Repositories;

namespace VaultApi.Application.Contratos;

public class ContratoService(IContratoRepository contratoRepository, IProdutoRepository produtoRepository)
{
    public async Task<ContratoAdminResponse> CriarAsync(CriarContratoRequest request)
    {
        var contratoId = Guid.NewGuid();
        var itens = new List<ContratoItem>();

        foreach (var itemRequest in request.Itens)
        {
            var itemId = Guid.NewGuid();
            itens.Add(new ContratoItem
            {
                Id = itemId,
                ContratoId = contratoId,
                ProdutoId = itemRequest.ProdutoId,
                ValorAdesaoOverride = itemRequest.ValorAdesaoOverride,
                ValorMensalidadeOverride = itemRequest.ValorMensalidadeOverride,
                TipoDesconto = itemRequest.TipoDesconto,
                ValorDesconto = itemRequest.ValorDesconto,
                Unidades = itemRequest.Unidades.Select(u => new ContratoItemUnidade { Id = Guid.NewGuid(), ContratoItemId = itemId, TipoUnidade = u.TipoUnidade, Quantidade = u.Quantidade }).ToList(),
                Modulos = itemRequest.Modulos.Select(m => new ContratoItemModulo { Id = Guid.NewGuid(), ContratoItemId = itemId, ModuloId = m.ModuloId, ModuloVarianteId = m.ModuloVarianteId, Ativo = m.Ativo, ValorOverride = m.ValorOverride }).ToList()
            });
        }

        var contrato = new Contrato { Id = contratoId, ClienteId = request.ClienteId, RevendaId = request.RevendaId, Ativo = true, DataInicio = request.DataInicio, Itens = itens };

        await contratoRepository.AddAsync(contrato);
        await contratoRepository.SaveChangesAsync();

        return await MontarAdminResponseAsync(contrato);
    }

    public async Task<ContratoAdminResponse?> ObterAdminAsync(Guid id)
    {
        var contrato = await contratoRepository.GetAsync(id);
        return contrato is null ? null : await MontarAdminResponseAsync(contrato);
    }

    public async Task<ContratoPublicoResponse?> ObterPublicoAsync(Guid id, ScopeResult scope)
    {
        var contrato = await contratoRepository.GetAsync(id);
        if (contrato is null || !PertenceAoEscopo(contrato, scope))
        {
            return null;
        }

        return new ContratoPublicoResponse(contrato.Id, contrato.ClienteId, contrato.RevendaId, contrato.Ativo, contrato.DataInicio, contrato.DataFim,
            contrato.Itens.Select(i => new ContratoItemPublicoResponse(i.Id, i.ProdutoId,
                i.Unidades.Select(u => new UnidadeRequest(u.TipoUnidade, u.Quantidade)).ToList(),
                i.Modulos.Where(m => m.Ativo).Select(m => m.ModuloId).ToList())).ToList());
    }

    private static bool PertenceAoEscopo(Contrato contrato, ScopeResult scope) => scope switch
    {
        ScopeResult.SemRestricao => true,
        ScopeResult.RestritoARevenda r => contrato.RevendaId == r.RevendaId,
        _ => false
    };

    private async Task<ContratoAdminResponse> MontarAdminResponseAsync(Contrato contrato)
    {
        var itensResponse = new List<ContratoItemAdminResponse>();

        foreach (var item in contrato.Itens)
        {
            var produto = await produtoRepository.GetAsync(item.ProdutoId) ?? throw new InvalidOperationException("Produto do item nao encontrado.");

            var precosPorUnidade = produto.PrecosPorUnidade.ToDictionary(p => p.TipoUnidade, p => (p.ValorAdesao, p.ValorMensalidade));
            var quantidades = item.Unidades.ToDictionary(u => u.TipoUnidade, u => u.Quantidade);

            var modulosInput = item.Modulos.Select(m =>
            {
                var modulo = produto.Modulos.Single(x => x.Id == m.ModuloId);
                var variante = m.ModuloVarianteId is { } varianteId ? modulo.Variantes.Single(v => v.Id == varianteId) : null;
                return new ModuloPricingInput(m.Ativo, modulo.ValorAdesaoBase, modulo.ValorMensalidadeBase,
                    variante?.TipoUnidadeAplicavel, variante?.ValorAdicionalPorUnidade, m.ValorOverride);
            }).ToList();

            var resultado = PricingResolver.Resolver(new ItemPricingInput(precosPorUnidade, quantidades, modulosInput,
                item.ValorAdesaoOverride, item.ValorMensalidadeOverride, item.TipoDesconto, item.ValorDesconto));

            itensResponse.Add(new ContratoItemAdminResponse(item.Id, item.ProdutoId,
                item.Unidades.Select(u => new UnidadeRequest(u.TipoUnidade, u.Quantidade)).ToList(),
                resultado.ValorAdesaoTotal, resultado.ValorMensalidadeTotal,
                item.ValorAdesaoOverride, item.ValorMensalidadeOverride, item.TipoDesconto, item.ValorDesconto));
        }

        return new ContratoAdminResponse(contrato.Id, contrato.ClienteId, contrato.RevendaId, contrato.Ativo, contrato.DataInicio, contrato.DataFim, itensResponse);
    }
}

using VaultApi.Domain.Enums;

namespace VaultApi.Application.Pricing;

public record ModuloPricingInput(
    bool Ativo,
    decimal? ValorAdesaoBase,
    decimal? ValorMensalidadeBase,
    TipoUnidade? VarianteTipoUnidadeAplicavel,
    decimal? ValorAdicionalPorUnidade,
    decimal? ValorOverride);

public record ItemPricingInput(
    IReadOnlyDictionary<TipoUnidade, (decimal Adesao, decimal Mensalidade)> PrecosPorUnidade,
    IReadOnlyDictionary<TipoUnidade, int> Quantidades,
    IReadOnlyList<ModuloPricingInput> Modulos,
    decimal? ValorAdesaoOverride,
    decimal? ValorMensalidadeOverride,
    TipoDesconto? TipoDesconto,
    decimal? ValorDesconto);

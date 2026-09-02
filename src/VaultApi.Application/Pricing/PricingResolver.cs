using VaultApi.Domain.Enums;

namespace VaultApi.Application.Pricing;

public static class PricingResolver
{
    public static ItemPricingResult Resolver(ItemPricingInput input)
    {
        var (adesaoBase, mensalidadeBase) = CalcularBaseProduto(input);
        var (adesaoModulos, mensalidadeModulos) = CalcularBaseModulos(input);

        var adesaoTotal = adesaoBase + adesaoModulos;
        var mensalidadeTotal = mensalidadeBase + mensalidadeModulos;

        if (input.TipoDesconto == Domain.Enums.TipoDesconto.Fixo)
        {
            return new ItemPricingResult(input.ValorAdesaoOverride ?? adesaoTotal, input.ValorMensalidadeOverride ?? mensalidadeTotal);
        }

        if (input.TipoDesconto == Domain.Enums.TipoDesconto.Percentual && input.ValorDesconto is { } percentual)
        {
            var fator = 1 - percentual / 100m;
            return new ItemPricingResult(adesaoTotal * fator, mensalidadeTotal * fator);
        }

        if (input.TipoDesconto == Domain.Enums.TipoDesconto.Valor && input.ValorDesconto is { } valor)
        {
            return new ItemPricingResult(adesaoTotal - valor, mensalidadeTotal - valor);
        }

        return new ItemPricingResult(adesaoTotal, mensalidadeTotal);
    }

    private static (decimal Adesao, decimal Mensalidade) CalcularBaseProduto(ItemPricingInput input)
    {
        decimal adesao = 0, mensalidade = 0;
        foreach (var (tipoUnidade, quantidade) in input.Quantidades)
        {
            if (!input.PrecosPorUnidade.TryGetValue(tipoUnidade, out var preco))
            {
                continue;
            }

            adesao += preco.Adesao * quantidade;
            mensalidade += preco.Mensalidade * quantidade;
        }

        return (adesao, mensalidade);
    }

    private static (decimal Adesao, decimal Mensalidade) CalcularBaseModulos(ItemPricingInput input)
    {
        decimal adesao = 0, mensalidade = 0;
        foreach (var modulo in input.Modulos.Where(m => m.Ativo))
        {
            adesao += modulo.ValorAdesaoBase ?? 0m;
            mensalidade += modulo.ValorMensalidadeBase ?? 0m;

            if (modulo.VarianteTipoUnidadeAplicavel is { } tipoUnidade
                && modulo.ValorAdicionalPorUnidade is { } valorAdicional
                && input.Quantidades.TryGetValue(tipoUnidade, out var quantidade))
            {
                mensalidade += valorAdicional * quantidade;
            }
        }

        return (adesao, mensalidade);
    }
}

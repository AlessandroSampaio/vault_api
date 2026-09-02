using FluentAssertions;
using VaultApi.Application.Pricing;
using VaultApi.Domain.Enums;

namespace VaultApi.Tests.Pricing;

public class PricingResolverTests
{
    [Fact]
    public void Calculates_base_price_from_unit_quantities_times_unit_price()
    {
        var input = new ItemPricingInput(
            PrecosPorUnidade: new Dictionary<TipoUnidade, (decimal Adesao, decimal Mensalidade)>
            {
                [TipoUnidade.Servidor] = (500m, 100m),
                [TipoUnidade.PDV] = (50m, 20m)
            },
            Quantidades: new Dictionary<TipoUnidade, int> { [TipoUnidade.Servidor] = 1, [TipoUnidade.PDV] = 5 },
            Modulos: [],
            ValorAdesaoOverride: null,
            ValorMensalidadeOverride: null,
            TipoDesconto: null,
            ValorDesconto: null);

        var result = PricingResolver.Resolver(input);

        // 1 servidor (500 adesao / 100 mensal) + 5 pdv (50 adesao / 20 mensal each)
        result.ValorAdesaoTotal.Should().Be(500m + 5 * 50m);
        result.ValorMensalidadeTotal.Should().Be(100m + 5 * 20m);
    }

    [Fact]
    public void Adds_flat_modulo_price_and_variante_price_per_applicable_unit()
    {
        var input = new ItemPricingInput(
            PrecosPorUnidade: new Dictionary<TipoUnidade, (decimal, decimal)> { [TipoUnidade.PDV] = (0m, 0m) },
            Quantidades: new Dictionary<TipoUnidade, int> { [TipoUnidade.PDV] = 5 },
            Modulos:
            [
                new ModuloPricingInput(Ativo: true, ValorAdesaoBase: 200m, ValorMensalidadeBase: 30m,
                    VarianteTipoUnidadeAplicavel: TipoUnidade.PDV, ValorAdicionalPorUnidade: 15m, ValorOverride: null)
            ],
            ValorAdesaoOverride: null, ValorMensalidadeOverride: null, TipoDesconto: null, ValorDesconto: null);

        var result = PricingResolver.Resolver(input);

        // modulo flat (200/30) + variante 15 x 5 pdv = 75 additional monthly-side charge only (adesao has no variante split here)
        result.ValorAdesaoTotal.Should().Be(200m);
        result.ValorMensalidadeTotal.Should().Be(30m + 5 * 15m);
    }

    [Fact]
    public void Ignores_inactive_modulo()
    {
        var input = new ItemPricingInput(
            PrecosPorUnidade: new Dictionary<TipoUnidade, (decimal, decimal)>(),
            Quantidades: new Dictionary<TipoUnidade, int>(),
            Modulos: [ new ModuloPricingInput(Ativo: false, ValorAdesaoBase: 999m, ValorMensalidadeBase: 999m,
                VarianteTipoUnidadeAplicavel: null, ValorAdicionalPorUnidade: null, ValorOverride: null) ],
            ValorAdesaoOverride: null, ValorMensalidadeOverride: null, TipoDesconto: null, ValorDesconto: null);

        var result = PricingResolver.Resolver(input);

        result.ValorAdesaoTotal.Should().Be(0m);
        result.ValorMensalidadeTotal.Should().Be(0m);
    }

    [Fact]
    public void Fixo_override_replaces_calculated_total()
    {
        var input = new ItemPricingInput(
            PrecosPorUnidade: new Dictionary<TipoUnidade, (decimal, decimal)> { [TipoUnidade.PDV] = (50m, 20m) },
            Quantidades: new Dictionary<TipoUnidade, int> { [TipoUnidade.PDV] = 10 },
            Modulos: [],
            ValorAdesaoOverride: 300m, ValorMensalidadeOverride: 120m,
            TipoDesconto: TipoDesconto.Fixo, ValorDesconto: null);

        var result = PricingResolver.Resolver(input);

        result.ValorAdesaoTotal.Should().Be(300m);
        result.ValorMensalidadeTotal.Should().Be(120m);
    }

    [Fact]
    public void Percentual_discount_applies_over_calculated_base()
    {
        var input = new ItemPricingInput(
            PrecosPorUnidade: new Dictionary<TipoUnidade, (decimal, decimal)> { [TipoUnidade.PDV] = (100m, 40m) },
            Quantidades: new Dictionary<TipoUnidade, int> { [TipoUnidade.PDV] = 1 },
            Modulos: [],
            ValorAdesaoOverride: null, ValorMensalidadeOverride: null,
            TipoDesconto: TipoDesconto.Percentual, ValorDesconto: 10m);

        var result = PricingResolver.Resolver(input);

        result.ValorAdesaoTotal.Should().Be(90m);
        result.ValorMensalidadeTotal.Should().Be(36m);
    }

    [Fact]
    public void Valor_discount_subtracts_fixed_amount_from_base()
    {
        var input = new ItemPricingInput(
            PrecosPorUnidade: new Dictionary<TipoUnidade, (decimal, decimal)> { [TipoUnidade.PDV] = (100m, 40m) },
            Quantidades: new Dictionary<TipoUnidade, int> { [TipoUnidade.PDV] = 1 },
            Modulos: [],
            ValorAdesaoOverride: null, ValorMensalidadeOverride: null,
            TipoDesconto: TipoDesconto.Valor, ValorDesconto: 15m);

        var result = PricingResolver.Resolver(input);

        result.ValorAdesaoTotal.Should().Be(85m);
        result.ValorMensalidadeTotal.Should().Be(25m);
    }
}

using VaultApi.Domain.Enums;
using FluentAssertions;

namespace VaultApi.Tests.Domain;

public class EnumsTests
{
    [Fact]
    public void Nivel_has_exactly_three_values()
    {
        Enum.GetValues<Nivel>().Should().BeEquivalentTo([Nivel.Admin, Nivel.Revenda, Nivel.Usuario]);
    }

    [Fact]
    public void TipoUnidade_has_exactly_four_values()
    {
        Enum.GetValues<TipoUnidade>().Should()
            .BeEquivalentTo([TipoUnidade.Servidor, TipoUnidade.Estacao, TipoUnidade.PDA, TipoUnidade.PDV]);
    }
}

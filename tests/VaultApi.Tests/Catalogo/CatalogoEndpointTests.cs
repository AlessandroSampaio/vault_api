using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VaultApi.Domain.Enums;
using VaultApi.Infrastructure.Persistence;
using VaultApi.Tests.Fixtures;

namespace VaultApi.Tests.Catalogo;

[Collection("Postgres")]
public class CatalogoEndpointTests(PostgresFixture postgres)
{
    [Fact]
    public async Task Admin_can_create_produto_with_precos_por_unidade_and_modulo_com_variante()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(postgres.ConnectionString).UseSnakeCaseNamingConvention().Options;
        await using (var db = new AppDbContext(options))
        {
            await db.Database.EnsureDeletedAsync();
            await db.Database.MigrateAsync();
        }

        await using var factory = new ApiFactory(postgres.ConnectionString);
        var token = await TestAuth.CreateUserAndLoginAsync(factory, Nivel.Admin, revendaId: null);
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var criarProdutoResponse = await client.PostAsJsonAsync("/produtos", new
        {
            nome = "MidPRO",
            descricao = "Produto de teste",
            precosPorUnidade = new[]
            {
                new { tipoUnidade = "Servidor", valorAdesao = 500m, valorMensalidade = 100m },
                new { tipoUnidade = "PDV", valorAdesao = 50m, valorMensalidade = 20m }
            }
        });
        criarProdutoResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var produto = await criarProdutoResponse.Content.ReadFromJsonAsync<ProdutoResponse>();

        var criarModuloResponse = await client.PostAsJsonAsync($"/produtos/{produto!.Id}/modulos", new
        {
            nome = "TEF",
            valorAdesaoBase = (decimal?)null,
            valorMensalidadeBase = (decimal?)null,
            variantes = new[]
            {
                new { nome = "TEF-Sitef", tipoUnidadeAplicavel = "PDV", valorAdicionalPorUnidade = 15m }
            }
        });
        criarModuloResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var produtoCompleto = await client.GetFromJsonAsync<ProdutoResponse>($"/produtos/{produto.Id}");
        produtoCompleto!.PrecosPorUnidade.Should().HaveCount(2);
        produtoCompleto.Modulos.Should().ContainSingle(m => m.Nome == "TEF" && m.Variantes.Count == 1);
    }

    private record ProdutoResponse(Guid Id, string Nome, string Descricao, bool Ativo,
        List<PrecoUnidadeResponse> PrecosPorUnidade, List<ModuloResponse> Modulos);
    private record PrecoUnidadeResponse(string TipoUnidade, decimal ValorAdesao, decimal ValorMensalidade);
    private record ModuloResponse(Guid Id, string Nome, decimal? ValorAdesaoBase, decimal? ValorMensalidadeBase,
        bool Ativo, List<VarianteResponse> Variantes);
    private record VarianteResponse(Guid Id, string Nome, string TipoUnidadeAplicavel, decimal ValorAdicionalPorUnidade);
}

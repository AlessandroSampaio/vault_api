using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VaultApi.Domain.Entities;
using VaultApi.Domain.Enums;
using VaultApi.Infrastructure.Persistence;
using VaultApi.Tests.Fixtures;

namespace VaultApi.Tests.Contratos;

[Collection("Postgres")]
public class ContratoEndpointTests(PostgresFixture postgres)
{
    [Fact]
    public async Task Admin_sees_prices_revenda_does_not()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(postgres.ConnectionString).UseSnakeCaseNamingConvention().Options;
        await using (var db = new AppDbContext(options))
        {
            await db.Database.EnsureDeletedAsync();
            await db.Database.MigrateAsync();
        }

        var revendaId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var produtoId = Guid.NewGuid();
        await using (var db = new AppDbContext(options))
        {
            db.Set<Revenda>().Add(new Revenda { Id = revendaId, RazaoSocial = "R", Cnpj = "1", Ativo = true });
            db.Set<Cliente>().Add(new Cliente { Id = clienteId, RazaoSocial = "C", Cnpj = "2", RevendaId = revendaId });
            db.Set<Produto>().Add(new Produto
            {
                Id = produtoId, Nome = "P", Descricao = "D", Ativo = true,
                PrecosPorUnidade = [ new ProdutoPrecoUnidade { Id = Guid.NewGuid(), ProdutoId = produtoId, TipoUnidade = TipoUnidade.PDV, ValorAdesao = 50m, ValorMensalidade = 20m } ]
            });
            await db.SaveChangesAsync();
        }

        await using var factory = new ApiFactory(postgres.ConnectionString);
        var adminToken = await TestAuth.CreateUserAndLoginAsync(factory, Nivel.Admin, revendaId: null);
        using var adminClient = factory.CreateClient();
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var criarResponse = await adminClient.PostAsJsonAsync("/contratos", new
        {
            clienteId, revendaId, dataInicio = DateOnly.FromDateTime(DateTime.UtcNow),
            itens = new[]
            {
                new
                {
                    produtoId,
                    unidades = new[] { new { tipoUnidade = "PDV", quantidade = 3 } },
                    modulos = Array.Empty<object>(),
                    valorAdesaoOverride = (decimal?)null, valorMensalidadeOverride = (decimal?)null,
                    tipoDesconto = (string?)null, valorDesconto = (decimal?)null
                }
            }
        });
        criarResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var contrato = await criarResponse.Content.ReadFromJsonAsync<ContratoAdminDto>();

        var adminGet = await adminClient.GetFromJsonAsync<ContratoAdminDto>($"/contratos/{contrato!.Id}");
        adminGet!.Itens[0].ValorMensalidadeCalculado.Should().Be(60m); // 3 pdv x 20

        var revendaToken = await TestAuth.CreateUserAndLoginAsync(factory, Nivel.Revenda, revendaId);
        using var revendaClient = factory.CreateClient();
        revendaClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", revendaToken);

        var revendaGetResponse = await revendaClient.GetAsync($"/contratos/{contrato.Id}");
        revendaGetResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await revendaGetResponse.Content.ReadAsStringAsync();
        body.Should().NotContain("valorMensalidadeCalculado", "revenda nunca deve ver valores negociados");
        body.Should().Contain("produtoId");
    }

    private record ContratoAdminDto(Guid Id, List<ContratoItemAdminDto> Itens);
    private record ContratoItemAdminDto(Guid ProdutoId, decimal ValorAdesaoCalculado, decimal ValorMensalidadeCalculado);
}

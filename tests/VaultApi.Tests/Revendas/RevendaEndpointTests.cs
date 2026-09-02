using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VaultApi.Domain.Enums;
using VaultApi.Infrastructure.Persistence;
using VaultApi.Tests.Fixtures;

namespace VaultApi.Tests.Revendas;

[Collection("Postgres")]
public class RevendaEndpointTests(PostgresFixture postgres)
{
    [Fact]
    public async Task Admin_can_create_and_list_revendas()
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

        var createResponse = await client.PostAsJsonAsync("/revendas", new { nome = "Revenda Teste", cnpj = "00000000000100" });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var listResponse = await client.GetFromJsonAsync<List<RevendaDto>>("/revendas");
        listResponse.Should().ContainSingle(r => r.Nome == "Revenda Teste");
    }

    [Fact]
    public async Task NonAdmin_cannot_create_revenda()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(postgres.ConnectionString).UseSnakeCaseNamingConvention().Options;
        await using (var db = new AppDbContext(options))
        {
            await db.Database.EnsureDeletedAsync();
            await db.Database.MigrateAsync();
        }

        await using var factory = new ApiFactory(postgres.ConnectionString);
        var token = await TestAuth.CreateUserAndLoginAsync(factory, Nivel.Revenda, revendaId: Guid.NewGuid());

        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync("/revendas", new { nome = "X", cnpj = "1" });
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private record RevendaDto(Guid Id, string Nome, string Cnpj, bool Ativo);
}

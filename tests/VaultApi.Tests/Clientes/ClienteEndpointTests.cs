using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VaultApi.Domain.Enums;
using VaultApi.Infrastructure.Persistence;
using VaultApi.Tests.Fixtures;

namespace VaultApi.Tests.Clientes;

[Collection("Postgres")]
public class ClienteEndpointTests(PostgresFixture postgres)
{
    [Fact]
    public async Task Admin_can_create_cliente_with_and_without_revenda()
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

        var matrizResponse = await client.PostAsJsonAsync("/clientes", new { nome = "Cliente Matriz", cnpj = "1", revendaId = (Guid?)null });
        matrizResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var revendaId = Guid.NewGuid();
        await using (var db = new AppDbContext(options))
        {
            db.Set<VaultApi.Domain.Entities.Revenda>().Add(new VaultApi.Domain.Entities.Revenda { Id = revendaId, Nome = "R1", Cnpj = "2", Ativo = true });
            await db.SaveChangesAsync();
        }

        var comRevendaResponse = await client.PostAsJsonAsync("/clientes", new { nome = "Cliente Revenda", cnpj = "3", revendaId });
        comRevendaResponse.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}

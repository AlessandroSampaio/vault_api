using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VaultApi.Domain.Entities;
using VaultApi.Domain.Enums;
using VaultApi.Infrastructure.Persistence;
using VaultApi.Tests.Fixtures;

namespace VaultApi.Tests.Clientes;

[Collection("Postgres")]
public class ClienteScopeEndpointTests(PostgresFixture postgres)
{
    [Fact]
    public async Task Revenda_user_only_sees_clientes_of_own_revenda()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(postgres.ConnectionString).UseSnakeCaseNamingConvention().Options;
        await using (var db = new AppDbContext(options))
        {
            await db.Database.EnsureDeletedAsync();
            await db.Database.MigrateAsync();
        }

        var revendaA = Guid.NewGuid();
        var revendaB = Guid.NewGuid();
        await using (var db = new AppDbContext(options))
        {
            db.Set<Revenda>().AddRange(
                new Revenda { Id = revendaA, RazaoSocial = "A", Cnpj = "1", Ativo = true },
                new Revenda { Id = revendaB, RazaoSocial = "B", Cnpj = "2", Ativo = true });
            db.Set<Cliente>().AddRange(
                new Cliente { Id = Guid.NewGuid(), RazaoSocial = "Cliente A", Cnpj = "10", RevendaId = revendaA },
                new Cliente { Id = Guid.NewGuid(), RazaoSocial = "Cliente B", Cnpj = "20", RevendaId = revendaB },
                new Cliente { Id = Guid.NewGuid(), RazaoSocial = "Cliente Matriz", Cnpj = "30", RevendaId = null });
            await db.SaveChangesAsync();
        }

        await using var factory = new ApiFactory(postgres.ConnectionString);
        var token = await TestAuth.CreateUserAndLoginAsync(factory, Nivel.Revenda, revendaA);
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var clientes = await client.GetFromJsonAsync<List<ClienteDto>>("/clientes");

        clientes.Should().ContainSingle().Which.RazaoSocial.Should().Be("Cliente A");
    }

    private record ClienteDto(Guid Id, string RazaoSocial, string Cnpj, Guid? RevendaId);
}

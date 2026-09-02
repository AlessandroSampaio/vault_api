using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VaultApi.Domain.Entities;
using VaultApi.Domain.Enums;
using VaultApi.Infrastructure.Persistence;
using VaultApi.Tests.Fixtures;

namespace VaultApi.Tests.Auth;

[Collection("Postgres")]
public class AuthEndpointTests(PostgresFixture postgres)
{
    [Fact]
    public async Task Login_returns_jwt_with_nivel_and_revenda_claims()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(postgres.ConnectionString).UseSnakeCaseNamingConvention().Options;
        await using (var db = new AppDbContext(options))
        {
            await db.Database.EnsureDeletedAsync();
            await db.Database.MigrateAsync();
        }

        await using var factory = new ApiFactory(postgres.ConnectionString);
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider
            .GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<Usuario>>();
        var revendaId = Guid.NewGuid();
        await userManager.CreateAsync(new Usuario
        {
            UserName = "admin@teste.com",
            Email = "admin@teste.com",
            Nome = "Admin",
            Nivel = Nivel.Admin,
            RevendaId = null
        }, "Senha123!");

        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/auth/login", new { email = "admin@teste.com", senha = "Senha123!" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        body!.Token.Should().NotBeNullOrEmpty();
    }

    private record LoginResponse(string Token);
}

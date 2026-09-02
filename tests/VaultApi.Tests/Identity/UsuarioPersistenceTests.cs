using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VaultApi.Domain.Entities;
using VaultApi.Domain.Enums;
using VaultApi.Infrastructure.Persistence;
using VaultApi.Tests.Fixtures;

namespace VaultApi.Tests.Identity;

[Collection("Postgres")]
public class UsuarioPersistenceTests(PostgresFixture postgres)
{
    [Fact]
    public async Task Usuario_roundtrips_with_nivel_and_revenda_id()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(postgres.ConnectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        await using (var db = new AppDbContext(options))
        {
            await db.Database.EnsureDeletedAsync();
            await db.Database.MigrateAsync();
        }

        var revendaId = Guid.NewGuid();

        await using (var db = new AppDbContext(options))
        {
            db.Users.Add(new Usuario
            {
                Id = Guid.NewGuid(),
                UserName = "revenda@teste.com",
                Email = "revenda@teste.com",
                Nome = "Usuario Revenda",
                Nivel = Nivel.Revenda,
                RevendaId = revendaId
            });
            await db.SaveChangesAsync();
        }

        await using (var db = new AppDbContext(options))
        {
            var usuario = await db.Users.SingleAsync();
            usuario.Nivel.Should().Be(Nivel.Revenda);
            usuario.RevendaId.Should().Be(revendaId);
        }
    }
}

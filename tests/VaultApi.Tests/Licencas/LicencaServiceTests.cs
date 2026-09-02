using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VaultApi.Domain.Entities;
using VaultApi.Domain.Enums;
using VaultApi.Infrastructure.Persistence;
using VaultApi.Infrastructure.Repositories;
using VaultApi.Tests.Fixtures;

namespace VaultApi.Tests.Licencas;

[Collection("Postgres")]
public class LicencaServiceTests(PostgresFixture postgres)
{
    [Fact]
    public async Task Emitting_a_new_version_revokes_the_previous_active_license()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(postgres.ConnectionString).UseSnakeCaseNamingConvention().Options;
        await using (var db = new AppDbContext(options))
        {
            await db.Database.EnsureDeletedAsync();
            await db.Database.MigrateAsync();
        }

        var contratoId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var item = new ContratoItem
        {
            Id = itemId, ContratoId = contratoId, ProdutoId = Guid.NewGuid(),
            Unidades = [ new ContratoItemUnidade { Id = Guid.NewGuid(), ContratoItemId = itemId, TipoUnidade = TipoUnidade.PDV, Quantidade = 5 } ],
            Modulos = []
        };

        await using (var db = new AppDbContext(options))
        {
            db.Set<Cliente>().Add(new Cliente { Id = Guid.NewGuid(), Nome = "C", Cnpj = "1", RevendaId = null });
            await db.SaveChangesAsync();
        }

        Guid primeiraLicencaId;
        await using (var db = new AppDbContext(options))
        {
            var service = new VaultApi.Application.Licencas.LicencaService(new LicencaRepository(db));
            var primeira = await service.EmitirNovaVersaoAsync(item);
            await db.SaveChangesAsync();
            primeiraLicencaId = primeira.Id;
        }

        await using (var db = new AppDbContext(options))
        {
            var service = new VaultApi.Application.Licencas.LicencaService(new LicencaRepository(db));
            item.Unidades[0].Quantidade = 8;
            await service.EmitirNovaVersaoAsync(item);
            await db.SaveChangesAsync();
        }

        await using var readDb = new AppDbContext(options);
        var licencas = await readDb.Set<Licenca>().Where(l => l.ContratoItemId == itemId).ToListAsync();

        licencas.Should().HaveCount(2);
        licencas.Single(l => l.Id == primeiraLicencaId).Status.Should().Be(StatusLicenca.Revogada);
        licencas.Single(l => l.Id != primeiraLicencaId).Status.Should().Be(StatusLicenca.Ativa);
    }
}

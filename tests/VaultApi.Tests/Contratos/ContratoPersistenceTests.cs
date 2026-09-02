using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VaultApi.Domain.Entities;
using VaultApi.Domain.Enums;
using VaultApi.Infrastructure.Persistence;
using VaultApi.Tests.Fixtures;

namespace VaultApi.Tests.Contratos;

[Collection("Postgres")]
public class ContratoPersistenceTests(PostgresFixture postgres)
{
    [Fact]
    public async Task Contrato_roundtrips_with_itens_unidades_and_modulos()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(postgres.ConnectionString).UseSnakeCaseNamingConvention().Options;
        await using (var db = new AppDbContext(options))
        {
            await db.Database.EnsureDeletedAsync();
            await db.Database.MigrateAsync();
        }

        var clienteId = Guid.NewGuid();
        var produtoId = Guid.NewGuid();
        var moduloId = Guid.NewGuid();
        await using (var db = new AppDbContext(options))
        {
            db.Set<Cliente>().Add(new Cliente { Id = clienteId, RazaoSocial = "Cliente", Cnpj = "1", RevendaId = null });
            db.Set<Produto>().Add(new Produto { Id = produtoId, Nome = "P", Descricao = "D", Ativo = true });
            db.Set<Modulo>().Add(new Modulo { Id = moduloId, ProdutoId = produtoId, Nome = "TEF", Ativo = true });
            await db.SaveChangesAsync();
        }

        var contratoId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        await using (var db = new AppDbContext(options))
        {
            db.Set<Contrato>().Add(new Contrato
            {
                Id = contratoId, ClienteId = clienteId, RevendaId = null, Ativo = true, DataInicio = DateOnly.FromDateTime(DateTime.UtcNow),
                Itens =
                [
                    new ContratoItem
                    {
                        Id = itemId, ContratoId = contratoId, ProdutoId = produtoId,
                        Unidades = [ new ContratoItemUnidade { Id = Guid.NewGuid(), ContratoItemId = itemId, TipoUnidade = TipoUnidade.PDV, Quantidade = 5 } ],
                        Modulos = [ new ContratoItemModulo { Id = Guid.NewGuid(), ContratoItemId = itemId, ModuloId = moduloId, Ativo = true } ]
                    }
                ]
            });
            await db.SaveChangesAsync();
        }

        await using var readDb = new AppDbContext(options);
        var contrato = await readDb.Set<Contrato>()
            .Include(c => c.Itens).ThenInclude(i => i.Unidades)
            .Include(c => c.Itens).ThenInclude(i => i.Modulos)
            .SingleAsync(c => c.Id == contratoId);

        contrato.Itens.Should().ContainSingle();
        contrato.Itens[0].Unidades.Should().ContainSingle(u => u.TipoUnidade == TipoUnidade.PDV && u.Quantidade == 5);
        contrato.Itens[0].Modulos.Should().ContainSingle(m => m.ModuloId == moduloId && m.Ativo);
    }
}

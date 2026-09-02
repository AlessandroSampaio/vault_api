using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using VaultApi.Domain.Entities;
using VaultApi.Domain.Enums;
using VaultApi.Infrastructure.Persistence;
using VaultApi.Tests.Fixtures;

namespace VaultApi.Tests.Catalogo;

[Collection("Postgres")]
public class HistoricoPrecoTests(PostgresFixture postgres)
{
    [Fact]
    public async Task Updating_produto_preco_unidade_appends_history_row()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(postgres.ConnectionString).UseSnakeCaseNamingConvention().Options;
        await using (var db = new AppDbContext(options))
        {
            await db.Database.EnsureDeletedAsync();
            await db.Database.MigrateAsync();
        }

        var produtoId = Guid.NewGuid();
        var precoId = Guid.NewGuid();
        await using (var db = new AppDbContext(options))
        {
            db.Set<Produto>().Add(new Produto { Id = produtoId, Nome = "P", Descricao = "D", Ativo = true });
            db.Set<ProdutoPrecoUnidade>().Add(new ProdutoPrecoUnidade
            {
                Id = precoId, ProdutoId = produtoId, TipoUnidade = TipoUnidade.PDV, ValorAdesao = 50m, ValorMensalidade = 20m
            });
            await db.SaveChangesAsync();
        }

        var usuarioId = Guid.NewGuid();
        await using (var scopeDb = new AppDbContext(options))
        {
            var repository = new VaultApi.Infrastructure.Repositories.ProdutoRepository(scopeDb);
            var historicoRepository = new VaultApi.Infrastructure.Repositories.HistoricoPrecoCatalogoRepository(scopeDb);
            var service = new VaultApi.Application.Catalogo.CatalogoService(repository, historicoRepository);

            await service.AtualizarPrecoUnidadeAsync(produtoId, TipoUnidade.PDV, novoValorAdesao: 60m, novoValorMensalidade: 25m, usuarioId);
        }

        await using var db2 = new AppDbContext(options);
        var historico = await db2.Set<HistoricoPrecoCatalogo>().ToListAsync();
        historico.Should().HaveCount(2);
        historico.Should().Contain(h => h.TipoValor == TipoValorCatalogo.Mensalidade && h.ValorAnterior == 20m && h.ValorNovo == 25m);
        historico.Should().Contain(h => h.TipoValor == TipoValorCatalogo.Adesao && h.ValorAnterior == 50m && h.ValorNovo == 60m);

        var precoAtualizado = await db2.Set<ProdutoPrecoUnidade>().SingleAsync(p => p.Id == precoId);
        precoAtualizado.ValorAdesao.Should().Be(60m);
        precoAtualizado.ValorMensalidade.Should().Be(25m);
    }
}

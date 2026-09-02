using VaultApi.Domain.Entities;
using VaultApi.Domain.Enums;
using VaultApi.Domain.Repositories;

namespace VaultApi.Application.Catalogo;

public class CatalogoService(IProdutoRepository repository, IHistoricoPrecoCatalogoRepository historicoRepository)
{
    public async Task<ProdutoResponse> CriarProdutoAsync(CriarProdutoRequest request)
    {
        var produto = new Produto
        {
            Id = Guid.NewGuid(),
            Nome = request.Nome,
            Descricao = request.Descricao,
            Ativo = true,
            PrecosPorUnidade = request.PrecosPorUnidade
                .Select(p => new ProdutoPrecoUnidade { Id = Guid.NewGuid(), TipoUnidade = p.TipoUnidade, ValorAdesao = p.ValorAdesao, ValorMensalidade = p.ValorMensalidade })
                .ToList()
        };

        await repository.AddAsync(produto);
        await repository.SaveChangesAsync();
        return ToResponse(produto);
    }

    public async Task<ModuloResponse> CriarModuloAsync(Guid produtoId, CriarModuloRequest request)
    {
        var produto = await repository.GetAsync(produtoId) ?? throw new KeyNotFoundException("Produto nao encontrado.");

        var modulo = new Modulo
        {
            Id = Guid.NewGuid(),
            ProdutoId = produtoId,
            Nome = request.Nome,
            ValorAdesaoBase = request.ValorAdesaoBase,
            ValorMensalidadeBase = request.ValorMensalidadeBase,
            Ativo = true,
            Variantes = request.Variantes
                .Select(v => new ModuloVariante { Id = Guid.NewGuid(), Nome = v.Nome, TipoUnidadeAplicavel = v.TipoUnidadeAplicavel, ValorAdicionalPorUnidade = v.ValorAdicionalPorUnidade })
                .ToList()
        };
        await repository.AddModuloAsync(modulo);

        await repository.SaveChangesAsync();
        return new ModuloResponse(modulo.Id, modulo.Nome, modulo.ValorAdesaoBase, modulo.ValorMensalidadeBase, modulo.Ativo,
            modulo.Variantes.Select(v => new VarianteResponse(v.Id, v.Nome, v.TipoUnidadeAplicavel, v.ValorAdicionalPorUnidade)).ToList());
    }

    public async Task<ProdutoResponse?> ObterProdutoAsync(Guid id)
    {
        var produto = await repository.GetAsync(id);
        return produto is null ? null : ToResponse(produto);
    }

    public async Task AtualizarPrecoUnidadeAsync(Guid produtoId, TipoUnidade tipoUnidade, decimal novoValorAdesao, decimal novoValorMensalidade, Guid usuarioId)
    {
        var produto = await repository.GetAsync(produtoId) ?? throw new KeyNotFoundException("Produto nao encontrado.");
        var preco = produto.PrecosPorUnidade.SingleOrDefault(p => p.TipoUnidade == tipoUnidade)
            ?? throw new KeyNotFoundException("Preco por unidade nao encontrado para este produto.");

        var agora = DateTimeOffset.UtcNow;

        if (preco.ValorAdesao != novoValorAdesao)
        {
            await historicoRepository.AddAsync(new HistoricoPrecoCatalogo
            {
                Id = Guid.NewGuid(), EntidadeTipo = EntidadeTipoCatalogo.ProdutoPrecoUnidade, EntidadeId = preco.Id,
                TipoValor = TipoValorCatalogo.Adesao, ValorAnterior = preco.ValorAdesao, ValorNovo = novoValorAdesao,
                DataAlteracao = agora, UsuarioId = usuarioId
            });
            preco.ValorAdesao = novoValorAdesao;
        }

        if (preco.ValorMensalidade != novoValorMensalidade)
        {
            await historicoRepository.AddAsync(new HistoricoPrecoCatalogo
            {
                Id = Guid.NewGuid(), EntidadeTipo = EntidadeTipoCatalogo.ProdutoPrecoUnidade, EntidadeId = preco.Id,
                TipoValor = TipoValorCatalogo.Mensalidade, ValorAnterior = preco.ValorMensalidade, ValorNovo = novoValorMensalidade,
                DataAlteracao = agora, UsuarioId = usuarioId
            });
            preco.ValorMensalidade = novoValorMensalidade;
        }

        await repository.SaveChangesAsync();
    }

    private static ProdutoResponse ToResponse(Produto produto) => new(
        produto.Id, produto.Nome, produto.Descricao, produto.Ativo,
        produto.PrecosPorUnidade.Select(p => new PrecoUnidadeResponse(p.TipoUnidade, p.ValorAdesao, p.ValorMensalidade)).ToList(),
        produto.Modulos.Select(m => new ModuloResponse(m.Id, m.Nome, m.ValorAdesaoBase, m.ValorMensalidadeBase, m.Ativo,
            m.Variantes.Select(v => new VarianteResponse(v.Id, v.Nome, v.TipoUnidadeAplicavel, v.ValorAdicionalPorUnidade)).ToList())).ToList());
}

using VaultApi.Domain.Enums;

namespace VaultApi.Application.Catalogo;

public record PrecoUnidadeRequest(TipoUnidade TipoUnidade, decimal ValorAdesao, decimal ValorMensalidade);
public record CriarProdutoRequest(string Nome, string Descricao, List<PrecoUnidadeRequest> PrecosPorUnidade);

public record VarianteRequest(string Nome, TipoUnidade TipoUnidadeAplicavel, decimal ValorAdicionalPorUnidade);
public record CriarModuloRequest(string Nome, decimal? ValorAdesaoBase, decimal? ValorMensalidadeBase, List<VarianteRequest> Variantes);

public record PrecoUnidadeResponse(TipoUnidade TipoUnidade, decimal ValorAdesao, decimal ValorMensalidade);
public record VarianteResponse(Guid Id, string Nome, TipoUnidade TipoUnidadeAplicavel, decimal ValorAdicionalPorUnidade);
public record ModuloResponse(Guid Id, string Nome, decimal? ValorAdesaoBase, decimal? ValorMensalidadeBase, bool Ativo, List<VarianteResponse> Variantes);
public record ProdutoResponse(Guid Id, string Nome, string Descricao, bool Ativo, List<PrecoUnidadeResponse> PrecosPorUnidade, List<ModuloResponse> Modulos);

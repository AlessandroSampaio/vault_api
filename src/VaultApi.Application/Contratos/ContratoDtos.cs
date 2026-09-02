using VaultApi.Domain.Enums;

namespace VaultApi.Application.Contratos;

public record UnidadeRequest(TipoUnidade TipoUnidade, int Quantidade);
public record ModuloItemRequest(Guid ModuloId, Guid? ModuloVarianteId, bool Ativo, decimal? ValorOverride);
public record ContratoItemRequest(
    Guid ProdutoId,
    List<UnidadeRequest> Unidades,
    List<ModuloItemRequest> Modulos,
    decimal? ValorAdesaoOverride,
    decimal? ValorMensalidadeOverride,
    TipoDesconto? TipoDesconto,
    decimal? ValorDesconto);
public record CriarContratoRequest(Guid ClienteId, Guid? RevendaId, DateOnly DataInicio, List<ContratoItemRequest> Itens);

public record ContratoItemAdminResponse(
    Guid Id, Guid ProdutoId, List<UnidadeRequest> Unidades,
    decimal ValorAdesaoCalculado, decimal ValorMensalidadeCalculado,
    decimal? ValorAdesaoOverride, decimal? ValorMensalidadeOverride, TipoDesconto? TipoDesconto, decimal? ValorDesconto);
public record ContratoAdminResponse(Guid Id, Guid ClienteId, Guid? RevendaId, bool Ativo, DateOnly DataInicio, DateOnly? DataFim, List<ContratoItemAdminResponse> Itens);

public record ContratoItemPublicoResponse(Guid Id, Guid ProdutoId, List<UnidadeRequest> Unidades, List<Guid> ModulosAtivos);
public record ContratoPublicoResponse(Guid Id, Guid ClienteId, Guid? RevendaId, bool Ativo, DateOnly DataInicio, DateOnly? DataFim, List<ContratoItemPublicoResponse> Itens);

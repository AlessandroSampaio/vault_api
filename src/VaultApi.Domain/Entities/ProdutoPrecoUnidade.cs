using VaultApi.Domain.Enums;

namespace VaultApi.Domain.Entities;

public class ProdutoPrecoUnidade
{
    public Guid Id { get; set; }
    public Guid ProdutoId { get; set; }
    public TipoUnidade TipoUnidade { get; set; }
    public decimal ValorAdesao { get; set; }
    public decimal ValorMensalidade { get; set; }
}

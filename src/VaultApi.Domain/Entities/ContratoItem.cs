using VaultApi.Domain.Enums;

namespace VaultApi.Domain.Entities;

public class ContratoItem
{
    public Guid Id { get; set; }
    public Guid ContratoId { get; set; }
    public Guid ProdutoId { get; set; }
    public decimal? ValorAdesaoOverride { get; set; }
    public decimal? ValorMensalidadeOverride { get; set; }
    public TipoDesconto? TipoDesconto { get; set; }
    public decimal? ValorDesconto { get; set; }
    public List<ContratoItemUnidade> Unidades { get; set; } = [];
    public List<ContratoItemModulo> Modulos { get; set; } = [];
}

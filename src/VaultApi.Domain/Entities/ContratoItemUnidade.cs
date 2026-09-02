using VaultApi.Domain.Enums;

namespace VaultApi.Domain.Entities;

public class ContratoItemUnidade
{
    public Guid Id { get; set; }
    public Guid ContratoItemId { get; set; }
    public TipoUnidade TipoUnidade { get; set; }
    public int Quantidade { get; set; }
}

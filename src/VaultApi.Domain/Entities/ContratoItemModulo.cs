namespace VaultApi.Domain.Entities;

public class ContratoItemModulo
{
    public Guid Id { get; set; }
    public Guid ContratoItemId { get; set; }
    public Guid ModuloId { get; set; }
    public Guid? ModuloVarianteId { get; set; }
    public bool Ativo { get; set; } = true;
    public decimal? ValorOverride { get; set; }
}

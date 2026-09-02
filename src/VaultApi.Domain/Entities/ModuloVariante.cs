using VaultApi.Domain.Enums;

namespace VaultApi.Domain.Entities;

public class ModuloVariante
{
    public Guid Id { get; set; }
    public Guid ModuloId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public TipoUnidade TipoUnidadeAplicavel { get; set; }
    public decimal ValorAdicionalPorUnidade { get; set; }
}

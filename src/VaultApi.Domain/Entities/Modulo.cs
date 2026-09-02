namespace VaultApi.Domain.Entities;

public class Modulo
{
    public Guid Id { get; set; }
    public Guid ProdutoId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal? ValorAdesaoBase { get; set; }
    public decimal? ValorMensalidadeBase { get; set; }
    public bool Ativo { get; set; } = true;
    public List<ModuloVariante> Variantes { get; set; } = [];
}

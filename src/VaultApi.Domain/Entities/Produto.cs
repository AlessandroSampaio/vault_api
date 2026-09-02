namespace VaultApi.Domain.Entities;

public class Produto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
    public List<ProdutoPrecoUnidade> PrecosPorUnidade { get; set; } = [];
    public List<Modulo> Modulos { get; set; } = [];
}

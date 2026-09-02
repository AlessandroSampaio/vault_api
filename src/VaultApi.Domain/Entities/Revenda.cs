namespace VaultApi.Domain.Entities;

public class Revenda
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cnpj { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
}

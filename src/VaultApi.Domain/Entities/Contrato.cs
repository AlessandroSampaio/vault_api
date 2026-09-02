namespace VaultApi.Domain.Entities;

public class Contrato
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public Guid? RevendaId { get; set; }
    public bool Ativo { get; set; } = true;
    public DateOnly DataInicio { get; set; }
    public DateOnly? DataFim { get; set; }
    public List<ContratoItem> Itens { get; set; } = [];
}

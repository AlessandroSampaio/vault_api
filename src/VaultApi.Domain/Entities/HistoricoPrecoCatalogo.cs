using VaultApi.Domain.Enums;

namespace VaultApi.Domain.Entities;

public class HistoricoPrecoCatalogo
{
    public Guid Id { get; set; }
    public EntidadeTipoCatalogo EntidadeTipo { get; set; }
    public Guid EntidadeId { get; set; }
    public TipoValorCatalogo TipoValor { get; set; }
    public decimal ValorAnterior { get; set; }
    public decimal ValorNovo { get; set; }
    public DateTimeOffset DataAlteracao { get; set; }
    public Guid UsuarioId { get; set; }
}

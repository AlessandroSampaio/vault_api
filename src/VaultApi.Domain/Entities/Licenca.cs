using VaultApi.Domain.Enums;

namespace VaultApi.Domain.Entities;

public class Licenca
{
    public Guid Id { get; set; }
    public Guid ContratoItemId { get; set; }
    public string Serial { get; set; } = string.Empty;
    public string Algoritmo { get; set; } = string.Empty;
    public DateTimeOffset DataEmissao { get; set; }
    public StatusLicenca Status { get; set; }
}

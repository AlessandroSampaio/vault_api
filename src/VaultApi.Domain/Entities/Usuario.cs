using Microsoft.AspNetCore.Identity;
using VaultApi.Domain.Enums;

namespace VaultApi.Domain.Entities;

public class Usuario : IdentityUser<Guid>
{
    public string Nome { get; set; } = string.Empty;
    public Nivel Nivel { get; set; }
    public Guid? RevendaId { get; set; }
}

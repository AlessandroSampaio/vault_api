using VaultApi.Domain.Enums;

namespace VaultApi.Application.Abstractions;

public interface ICurrentUser
{
    Nivel Nivel { get; }
    Guid? RevendaId { get; }
}

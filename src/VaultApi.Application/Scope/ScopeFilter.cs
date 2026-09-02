using VaultApi.Application.Abstractions;
using VaultApi.Domain.Enums;

namespace VaultApi.Application.Scope;

public class ScopeFilter(ICurrentUser currentUser) : IScopeFilter
{
    public ScopeResult Resolve() => currentUser.Nivel switch
    {
        Nivel.Admin => new ScopeResult.SemRestricao(),
        _ => new ScopeResult.RestritoARevenda(currentUser.RevendaId)
    };
}

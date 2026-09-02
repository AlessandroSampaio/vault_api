namespace VaultApi.Application.Abstractions;

public abstract record ScopeResult
{
    public sealed record SemRestricao : ScopeResult;
    public sealed record RestritoARevenda(Guid? RevendaId) : ScopeResult;
}

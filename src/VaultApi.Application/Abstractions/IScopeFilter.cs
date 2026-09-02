namespace VaultApi.Application.Abstractions;

public interface IScopeFilter
{
    ScopeResult Resolve();
}

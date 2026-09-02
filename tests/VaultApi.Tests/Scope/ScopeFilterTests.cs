using FluentAssertions;
using VaultApi.Application.Abstractions;
using VaultApi.Application.Scope;
using VaultApi.Domain.Enums;

namespace VaultApi.Tests.Scope;

public class ScopeFilterTests
{
    private class FakeCurrentUser(Nivel nivel, Guid? revendaId) : ICurrentUser
    {
        public Nivel Nivel => nivel;
        public Guid? RevendaId => revendaId;
    }

    [Fact]
    public void Admin_has_no_restriction()
    {
        var filter = new ScopeFilter(new FakeCurrentUser(Nivel.Admin, null));
        filter.Resolve().Should().BeOfType<ScopeResult.SemRestricao>();
    }

    [Fact]
    public void Revenda_user_is_restricted_to_own_revenda_id()
    {
        var revendaId = Guid.NewGuid();
        var filter = new ScopeFilter(new FakeCurrentUser(Nivel.Revenda, revendaId));
        var result = filter.Resolve().Should().BeOfType<ScopeResult.RestritoARevenda>().Subject;
        result.RevendaId.Should().Be(revendaId);
    }

    [Fact]
    public void Usuario_from_matriz_is_restricted_to_null_revenda_id()
    {
        var filter = new ScopeFilter(new FakeCurrentUser(Nivel.Usuario, null));
        var result = filter.Resolve().Should().BeOfType<ScopeResult.RestritoARevenda>().Subject;
        result.RevendaId.Should().BeNull();
    }
}

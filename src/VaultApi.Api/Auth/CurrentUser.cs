using Microsoft.AspNetCore.Http;
using VaultApi.Application.Abstractions;
using VaultApi.Domain.Enums;

namespace VaultApi.Api.Auth;

public class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public Nivel Nivel => Enum.Parse<Nivel>(
        accessor.HttpContext?.User.FindFirst("nivel")?.Value
        ?? throw new InvalidOperationException("Requisicao sem claim 'nivel'."));

    public Guid? RevendaId
    {
        get
        {
            var value = accessor.HttpContext?.User.FindFirst("revenda_id")?.Value;
            return value is null ? null : Guid.Parse(value);
        }
    }
}

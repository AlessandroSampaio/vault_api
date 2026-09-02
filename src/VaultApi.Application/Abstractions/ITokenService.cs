using VaultApi.Domain.Entities;

namespace VaultApi.Application.Abstractions;

public interface ITokenService
{
    string GerarToken(Usuario usuario);
}

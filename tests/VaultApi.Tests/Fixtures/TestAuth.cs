using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using VaultApi.Application.Abstractions;
using VaultApi.Domain.Entities;
using VaultApi.Domain.Enums;

namespace VaultApi.Tests.Fixtures;

public static class TestAuth
{
    public static async Task<string> CreateUserAndLoginAsync(ApiFactory factory, Nivel nivel, Guid? revendaId)
    {
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Usuario>>();
        var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();

        var email = $"{Guid.NewGuid()}@teste.com";
        var usuario = new Usuario { UserName = email, Email = email, Nome = "Teste", Nivel = nivel, RevendaId = revendaId };
        var result = await userManager.CreateAsync(usuario, "Senha123!");
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join(";", result.Errors.Select(e => e.Description)));
        }

        return tokenService.GerarToken(usuario);
    }
}

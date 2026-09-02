using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VaultApi.Application.Abstractions;
using VaultApi.Domain.Entities;

namespace VaultApi.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(UserManager<Usuario> userManager, ITokenService tokenService) : ControllerBase
{
    public record LoginRequest(string Email, string Senha);
    public record LoginResponse(string Token);

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var usuario = await userManager.FindByEmailAsync(request.Email);
        if (usuario is null || !await userManager.CheckPasswordAsync(usuario, request.Senha))
        {
            return Unauthorized();
        }

        return Ok(new LoginResponse(tokenService.GerarToken(usuario)));
    }
}

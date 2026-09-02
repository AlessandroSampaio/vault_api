using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VaultApi.Api.Auth;
using VaultApi.Application.Revendas;

namespace VaultApi.Api.Controllers;

[ApiController]
[Route("revendas")]
[Authorize]
public class RevendasController(RevendaService service) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = PolicyNames.RequireAdmin)]
    public async Task<ActionResult<RevendaResponse>> Criar(CriarRevendaRequest request)
    {
        var revenda = await service.CriarAsync(request);
        return CreatedAtAction(nameof(Listar), new { }, revenda);
    }

    [HttpGet]
    [Authorize(Policy = PolicyNames.RequireRevendaOrAdmin)]
    public async Task<ActionResult<List<RevendaResponse>>> Listar() => Ok(await service.ListarAsync());
}

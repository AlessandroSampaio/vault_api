using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VaultApi.Api.Auth;
using VaultApi.Application.Abstractions;
using VaultApi.Application.Contratos;

namespace VaultApi.Api.Controllers;

[ApiController]
[Route("contratos")]
[Authorize]
public class ContratosController(ContratoService service, IScopeFilter scopeFilter) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = PolicyNames.RequireAdmin)]
    public async Task<ActionResult<ContratoAdminResponse>> Criar(CriarContratoRequest request)
    {
        var contrato = await service.CriarAsync(request);
        return CreatedAtAction(nameof(Obter), new { id = contrato.Id }, contrato);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PolicyNames.RequireRevendaOrAdmin)]
    public async Task<IActionResult> Obter(Guid id)
    {
        var scope = scopeFilter.Resolve();

        if (scope is ScopeResult.SemRestricao)
        {
            var admin = await service.ObterAdminAsync(id);
            return admin is null ? NotFound() : Ok(admin);
        }

        var publico = await service.ObterPublicoAsync(id, scope);
        return publico is null ? NotFound() : Ok(publico);
    }
}

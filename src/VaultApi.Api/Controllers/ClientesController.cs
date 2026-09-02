using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VaultApi.Api.Auth;
using VaultApi.Application.Abstractions;
using VaultApi.Application.Clientes;

namespace VaultApi.Api.Controllers;

[ApiController]
[Route("clientes")]
[Authorize]
public class ClientesController(ClienteService service, IScopeFilter scopeFilter) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = PolicyNames.RequireAdmin)]
    public async Task<ActionResult<ClienteResponse>> Criar(CriarClienteRequest request)
    {
        var cliente = await service.CriarAsync(request);
        return Created($"/clientes/{cliente.Id}", cliente);
    }

    [HttpGet]
    [Authorize(Policy = PolicyNames.RequireRevendaOrAdmin)]
    public async Task<ActionResult<List<ClienteResponse>>> Listar() =>
        Ok(await service.ListarAsync(scopeFilter.Resolve()));
}

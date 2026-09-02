using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VaultApi.Api.Auth;
using VaultApi.Application.Clientes;

namespace VaultApi.Api.Controllers;

[ApiController]
[Route("clientes")]
[Authorize]
public class ClientesController(ClienteService service) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = PolicyNames.RequireAdmin)]
    public async Task<ActionResult<ClienteResponse>> Criar(CriarClienteRequest request)
    {
        var cliente = await service.CriarAsync(request);
        return Created($"/clientes/{cliente.Id}", cliente);
    }
}

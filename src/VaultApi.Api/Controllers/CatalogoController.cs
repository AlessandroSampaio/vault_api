using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VaultApi.Api.Auth;
using VaultApi.Application.Catalogo;

namespace VaultApi.Api.Controllers;

[ApiController]
[Route("produtos")]
[Authorize]
public class CatalogoController(CatalogoService service) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = PolicyNames.RequireAdmin)]
    public async Task<ActionResult<ProdutoResponse>> CriarProduto(CriarProdutoRequest request)
    {
        var produto = await service.CriarProdutoAsync(request);
        return CreatedAtAction(nameof(Obter), new { id = produto.Id }, produto);
    }

    [HttpPost("{id:guid}/modulos")]
    [Authorize(Policy = PolicyNames.RequireAdmin)]
    public async Task<ActionResult<ModuloResponse>> CriarModulo(Guid id, CriarModuloRequest request)
    {
        var modulo = await service.CriarModuloAsync(id, request);
        return Created($"/produtos/{id}/modulos/{modulo.Id}", modulo);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PolicyNames.RequireRevendaOrAdmin)]
    public async Task<ActionResult<ProdutoResponse>> Obter(Guid id)
    {
        var produto = await service.ObterProdutoAsync(id);
        return produto is null ? NotFound() : Ok(produto);
    }
}

using System.Security.Claims;
using EstanteDigital.Application.Excecoes;
using EstanteDigital.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EstanteDigital.Api.Controllers;

/// <summary>
/// Fase 3: tudo que envolve a relação entre contas (buscar gente, seguir,
/// ver o perfil público de outra conta) — separado de PerfilController de
/// propósito, porque aquele é só sobre editar a própria conta.
/// </summary>
[ApiController]
[Route("api/usuarios")]
[Authorize]
public class PerfilSocialController : ControllerBase
{
    private readonly IPerfilSocialService _service;

    public PerfilSocialController(IPerfilSocialService service)
    {
        _service = service;
    }

    private Guid UsuarioId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("buscar")]
    public async Task<IActionResult> Buscar([FromQuery] string q)
    {
        try
        {
            var encontrados = await _service.BuscarUsuariosAsync(UsuarioId, q);
            return Ok(encontrados);
        }
        catch (ValidacaoException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    [HttpGet("{nomeUsuario}/perfil-publico")]
    public async Task<IActionResult> ObterPerfilPublico(string nomeUsuario)
    {
        try
        {
            var perfil = await _service.ObterPerfilPublicoAsync(UsuarioId, nomeUsuario);
            return Ok(perfil);
        }
        catch (ValidacaoException ex)
        {
            return NotFound(new { mensagem = ex.Message });
        }
    }

    [HttpPost("{nomeUsuario}/seguir")]
    public async Task<IActionResult> Seguir(string nomeUsuario)
    {
        try
        {
            await _service.SeguirAsync(UsuarioId, nomeUsuario);
            return NoContent();
        }
        catch (ValidacaoException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    [HttpDelete("{nomeUsuario}/seguir")]
    public async Task<IActionResult> DeixarDeSeguir(string nomeUsuario)
    {
        try
        {
            await _service.DeixarDeSeguirAsync(UsuarioId, nomeUsuario);
            return NoContent();
        }
        catch (ValidacaoException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    [HttpGet("me/seguindo")]
    public async Task<IActionResult> ListarSeguindo()
    {
        var lista = await _service.ListarSeguindoAsync(UsuarioId);
        return Ok(lista);
    }

    [HttpGet("me/seguidores")]
    public async Task<IActionResult> ListarSeguidores()
    {
        var lista = await _service.ListarSeguidoresAsync(UsuarioId);
        return Ok(lista);
    }
}

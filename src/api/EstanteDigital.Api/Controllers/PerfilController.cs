using System.Security.Claims;
using EstanteDigital.Application.DTOs;
using EstanteDigital.Application.Excecoes;
using EstanteDigital.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EstanteDigital.Api.Controllers;

[ApiController]
[Route("api/perfil")]
[Authorize]
public class PerfilController : ControllerBase
{
    private readonly IPerfilService _service;

    public PerfilController(IPerfilService service)
    {
        _service = service;
    }

    private Guid UsuarioId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> Obter()
    {
        var perfil = await _service.ObterAsync(UsuarioId);
        return perfil is null ? NotFound() : Ok(perfil);
    }

    [HttpPut]
    public async Task<IActionResult> Atualizar([FromBody] AtualizarPerfilDto dto)
    {
        try
        {
            var atualizado = await _service.AtualizarAsync(UsuarioId, dto);
            return Ok(atualizado);
        }
        catch (ValidacaoException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    [HttpPut("senha")]
    public async Task<IActionResult> AlterarSenha([FromBody] AlterarSenhaDto dto)
    {
        try
        {
            await _service.AlterarSenhaAsync(UsuarioId, dto);
            return NoContent();
        }
        catch (ValidacaoException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }
}

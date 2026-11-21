using System.Security.Claims;
using EstanteDigital.Application.DTOs;
using EstanteDigital.Application.Excecoes;
using EstanteDigital.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EstanteDigital.Api.Controllers;

/// <summary>Fase 4: desafios de leitura em grupo — participação aberta
/// (ver DesafioService para o porquê).</summary>
[ApiController]
[Route("api/desafios")]
[Authorize]
public class DesafiosController : ControllerBase
{
    private readonly IDesafioService _service;

    public DesafiosController(IDesafioService service)
    {
        _service = service;
    }

    private Guid UsuarioId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> Listar() => Ok(await _service.ListarTodosAsync(UsuarioId));

    [HttpGet("meus")]
    public async Task<IActionResult> ListarMeus() => Ok(await _service.ListarMeusAsync(UsuarioId));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObterDetalhe(Guid id)
    {
        var detalhe = await _service.ObterDetalheAsync(id, UsuarioId);
        return detalhe is null ? NotFound(new { mensagem = "Desafio não encontrado." }) : Ok(detalhe);
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarDesafioDto dto)
    {
        try
        {
            var criado = await _service.CriarAsync(UsuarioId, dto);
            return CreatedAtAction(nameof(ObterDetalhe), new { id = criado.Id }, criado);
        }
        catch (ValidacaoException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    [HttpPost("{id:guid}/entrar")]
    public async Task<IActionResult> Entrar(Guid id)
    {
        try
        {
            await _service.EntrarAsync(id, UsuarioId);
            return NoContent();
        }
        catch (ValidacaoException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    [HttpDelete("{id:guid}/entrar")]
    public async Task<IActionResult> Sair(Guid id)
    {
        await _service.SairAsync(id, UsuarioId);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remover(Guid id)
    {
        try
        {
            var removido = await _service.RemoverAsync(id, UsuarioId);
            return removido ? NoContent() : NotFound(new { mensagem = "Desafio não encontrado." });
        }
        catch (ValidacaoException ex)
        {
            return StatusCode(403, new { mensagem = ex.Message });
        }
    }
}

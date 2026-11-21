using System.Security.Claims;
using EstanteDigital.Application.DTOs;
using EstanteDigital.Application.Excecoes;
using EstanteDigital.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EstanteDigital.Api.Controllers;

/// <summary>
/// Todo o controller exige login — não existe "catálogo público", porque
/// cada conta tem a própria estante privada (modelo multi-tenant). O id de
/// quem está logado vem da claim NameIdentifier do token (ver
/// BearerAuthenticationHandler) — nunca de um parâmetro vindo do cliente,
/// então não tem como uma requisição "fingir" ser outro usuário.
/// </summary>
[ApiController]
[Route("api/livros")]
[Authorize]
public class LivrosController : ControllerBase
{
    private readonly ILivroService _service;

    public LivrosController(ILivroService service)
    {
        _service = service;
    }

    private Guid UsuarioId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<LivroDto>>> Listar(
        [FromQuery] string? genero, [FromQuery] string? autor, [FromQuery] string? status, [FromQuery] string? busca)
    {
        var livros = await _service.ListarAsync(UsuarioId, genero, autor, status, busca);
        return Ok(livros);
    }

    [HttpGet("ranking")]
    public async Task<ActionResult<IReadOnlyList<LivroDto>>> ObterRanking()
    {
        var ranking = await _service.ObterRankingAsync(UsuarioId);
        return Ok(ranking);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<LivroDto>> BuscarPorId(Guid id)
    {
        var livro = await _service.BuscarPorIdAsync(id, UsuarioId);
        return livro is null ? NotFound(new { mensagem = "Livro não encontrado." }) : Ok(livro);
    }

    [HttpPost]
    public async Task<ActionResult<LivroDto>> Criar([FromBody] CriarLivroDto dto)
    {
        try
        {
            var criado = await _service.CriarAsync(UsuarioId, dto);
            return CreatedAtAction(nameof(BuscarPorId), new { id = criado.Id }, criado);
        }
        catch (ValidacaoException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<LivroDto>> Atualizar(Guid id, [FromBody] AtualizarLivroDto dto)
    {
        try
        {
            var atualizado = await _service.AtualizarAsync(id, UsuarioId, dto);
            return atualizado is null ? NotFound(new { mensagem = "Livro não encontrado." }) : Ok(atualizado);
        }
        catch (ValidacaoException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remover(Guid id)
    {
        var removido = await _service.RemoverAsync(id, UsuarioId);
        return removido ? NoContent() : NotFound(new { mensagem = "Livro não encontrado." });
    }
}

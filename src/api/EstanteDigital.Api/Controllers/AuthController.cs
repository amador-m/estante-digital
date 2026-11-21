using EstanteDigital.Application.DTOs;
using EstanteDigital.Application.Excecoes;
using EstanteDigital.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EstanteDigital.Api.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("registro")]
    public async Task<IActionResult> Registrar([FromBody] RegistroDto dto)
    {
        try
        {
            var resultado = await _authService.RegistrarAsync(dto);
            return Ok(resultado);
        }
        catch (ValidacaoException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var resultado = await _authService.LoginAsync(dto);
        return resultado is null
            ? Unauthorized(new { mensagem = "E-mail ou senha inválidos." })
            : Ok(resultado);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshDto dto)
    {
        var resultado = await _authService.RefreshAsync(dto.RefreshToken);
        return resultado is null
            ? Unauthorized(new { mensagem = "Refresh token inválido ou expirado." })
            : Ok(resultado);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshDto dto)
    {
        await _authService.LogoutAsync(dto.RefreshToken);
        return NoContent();
    }
}

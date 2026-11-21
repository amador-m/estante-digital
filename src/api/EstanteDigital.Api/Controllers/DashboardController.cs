using System.Security.Claims;
using EstanteDigital.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EstanteDigital.Api.Controllers;

/// <summary>Fase 2: dashboard agora é por conta — cada usuário só vê os
/// números da própria estante (ver LivrosController para o raciocínio
/// completo sobre a mudança de modelo público → privado).</summary>
[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly ILivroService _service;

    public DashboardController(ILivroService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Obter()
    {
        var usuarioId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var dashboard = await _service.ObterDashboardAsync(usuarioId);
        return Ok(dashboard);
    }
}

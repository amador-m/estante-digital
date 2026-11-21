using System.Security.Claims;
using EstanteDigital.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EstanteDigital.Api.Controllers;

/// <summary>Fase 4: streak de leitura (sequência de dias com check-in
/// manual) — ver StreakService para a regra de contagem.</summary>
[ApiController]
[Route("api/streak")]
[Authorize]
public class StreakController : ControllerBase
{
    private readonly IStreakService _service;

    public StreakController(IStreakService service)
    {
        _service = service;
    }

    private Guid UsuarioId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> ObterStatus() => Ok(await _service.ObterStatusAsync(UsuarioId));

    [HttpPost("checkin")]
    public async Task<IActionResult> RegistrarCheckIn() => Ok(await _service.RegistrarCheckInAsync(UsuarioId));
}

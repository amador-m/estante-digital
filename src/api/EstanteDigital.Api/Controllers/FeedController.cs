using System.Security.Claims;
using EstanteDigital.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EstanteDigital.Api.Controllers;

/// <summary>Fase 4: feed de atividade de quem o usuário segue — ver
/// FeedService para como os eventos são montados.</summary>
[ApiController]
[Route("api/feed")]
[Authorize]
public class FeedController : ControllerBase
{
    private readonly IFeedService _service;

    public FeedController(IFeedService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Obter()
    {
        var usuarioId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await _service.ObterFeedAsync(usuarioId));
    }
}

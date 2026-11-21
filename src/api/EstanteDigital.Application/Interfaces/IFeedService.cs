using EstanteDigital.Application.DTOs;

namespace EstanteDigital.Application.Interfaces;

public interface IFeedService
{
    /// <summary>Atividades de quem usuarioId segue (nunca as do próprio
    /// usuarioId) — mais recentes primeiro.</summary>
    Task<IReadOnlyList<AtividadeDto>> ObterFeedAsync(Guid usuarioId);
}

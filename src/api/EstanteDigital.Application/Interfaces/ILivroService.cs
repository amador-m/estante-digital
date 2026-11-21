using EstanteDigital.Application.DTOs;

namespace EstanteDigital.Application.Interfaces;

public interface ILivroService
{
    Task<IReadOnlyList<LivroDto>> ListarAsync(Guid usuarioId, string? genero, string? autor, string? status, string? busca);
    Task<LivroDto?> BuscarPorIdAsync(Guid id, Guid usuarioId);
    Task<LivroDto> CriarAsync(Guid usuarioId, CriarLivroDto dto);
    Task<LivroDto?> AtualizarAsync(Guid id, Guid usuarioId, AtualizarLivroDto dto);
    Task<bool> RemoverAsync(Guid id, Guid usuarioId);
    Task<DashboardDto> ObterDashboardAsync(Guid usuarioId);
    Task<IReadOnlyList<LivroDto>> ObterRankingAsync(Guid usuarioId);
}

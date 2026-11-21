using EstanteDigital.Application.DTOs;

namespace EstanteDigital.Application.Interfaces;

public interface IDesafioService
{
    Task<DesafioDto> CriarAsync(Guid criadorId, CriarDesafioDto dto);
    Task<IReadOnlyList<DesafioDto>> ListarTodosAsync(Guid usuarioId);
    Task<IReadOnlyList<DesafioDto>> ListarMeusAsync(Guid usuarioId);
    Task<DesafioDetalheDto?> ObterDetalheAsync(Guid id, Guid usuarioId);
    Task EntrarAsync(Guid id, Guid usuarioId);
    Task SairAsync(Guid id, Guid usuarioId);
    Task<bool> RemoverAsync(Guid id, Guid usuarioId);
}

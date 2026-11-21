using EstanteDigital.Application.DTOs;

namespace EstanteDigital.Application.Interfaces;

public interface IPerfilService
{
    Task<PerfilDto?> ObterAsync(Guid usuarioId);
    Task<PerfilDto> AtualizarAsync(Guid usuarioId, AtualizarPerfilDto dto);
    Task AlterarSenhaAsync(Guid usuarioId, AlterarSenhaDto dto);
}

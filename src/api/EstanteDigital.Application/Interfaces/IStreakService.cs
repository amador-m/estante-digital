using EstanteDigital.Application.DTOs;

namespace EstanteDigital.Application.Interfaces;

public interface IStreakService
{
    Task<StreakDto> ObterStatusAsync(Guid usuarioId);

    /// <summary>Idempotente: marcar duas vezes no mesmo dia não quebra nem
    /// duplica nada, só devolve o status atual.</summary>
    Task<StreakDto> RegistrarCheckInAsync(Guid usuarioId);
}

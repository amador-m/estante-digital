using EstanteDigital.Domain.Entidades;

namespace EstanteDigital.Domain.Repositorios;

public interface ICheckInRepository
{
    /// <summary>Idempotente por (usuarioId, data) — marcar duas vezes no
    /// mesmo dia não cria dois check-ins nem quebra o streak.</summary>
    Task<CheckInLeitura> RegistrarAsync(Guid usuarioId, DateTime data);

    Task<bool> JaRegistrouAsync(Guid usuarioId, DateTime data);

    /// <summary>Todas as datas com check-in deste usuário, pra
    /// StreakService calcular a sequência atual (ver lógica lá).</summary>
    Task<IReadOnlyList<DateTime>> ListarDatasAsync(Guid usuarioId);
}

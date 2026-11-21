using EstanteDigital.Domain.Entidades;

namespace EstanteDigital.Domain.Repositorios;

public interface IParticipacaoDesafioRepository
{
    /// <summary>Idempotente — entrar de novo num desafio que já participa
    /// simplesmente devolve a participação existente, sem duplicar (mesmo
    /// padrão do ISeguidorRepository.SeguirAsync).</summary>
    Task<ParticipacaoDesafio> EntrarAsync(Guid desafioId, Guid usuarioId);

    Task SairAsync(Guid desafioId, Guid usuarioId);
    Task<bool> ParticipaAsync(Guid desafioId, Guid usuarioId);

    Task<IReadOnlyList<Guid>> ListarParticipantesIdsAsync(Guid desafioId);

    /// <summary>Ids dos desafios em que o usuário participa (não só os que
    /// ele criou) — usado em "meus desafios" e no cálculo do feed.</summary>
    Task<IReadOnlyList<Guid>> ListarDesafioIdsDoUsuarioAsync(Guid usuarioId);

    Task RemoverTodasDoDesafioAsync(Guid desafioId);
}

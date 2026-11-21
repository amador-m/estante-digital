using EstanteDigital.Domain.Entidades;

namespace EstanteDigital.Domain.Repositorios;

public interface ISeguidorRepository
{
    /// <summary>Null se já não existir (operação idempotente — seguir
    /// quem já é seguido simplesmente não duplica a relação).</summary>
    Task<Seguidor?> SeguirAsync(Guid seguidorId, Guid seguidoId);

    /// <summary>Não lança erro se a relação não existir — deixar de seguir
    /// quem não se seguia não é um estado de erro.</summary>
    Task DeixarDeSeguirAsync(Guid seguidorId, Guid seguidoId);

    Task<bool> EstaSeguindoAsync(Guid seguidorId, Guid seguidoId);

    Task<int> ContarSeguidoresAsync(Guid usuarioId);
    Task<int> ContarSeguindoAsync(Guid usuarioId);

    /// <summary>Ids de quem o usuário segue.</summary>
    Task<IReadOnlyList<Guid>> ListarSeguindoIdsAsync(Guid usuarioId);

    /// <summary>Ids de quem segue o usuário.</summary>
    Task<IReadOnlyList<Guid>> ListarSeguidoresIdsAsync(Guid usuarioId);
}

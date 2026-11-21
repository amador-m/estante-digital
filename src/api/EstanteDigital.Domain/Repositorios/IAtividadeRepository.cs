using EstanteDigital.Domain.Entidades;

namespace EstanteDigital.Domain.Repositorios;

public interface IAtividadeRepository
{
    Task<Atividade> RegistrarAsync(Atividade atividade);

    /// <summary>Atividades de uma lista de usuários (tipicamente "quem eu
    /// sigo"), mais recentes primeiro, limitadas a `limite` registros —
    /// o feed não precisa (nem deveria) devolver o histórico inteiro.</summary>
    Task<IReadOnlyList<Atividade>> ListarDeUsuariosAsync(IReadOnlyList<Guid> usuarioIds, int limite);
}

using EstanteDigital.Application.DTOs;

namespace EstanteDigital.Application.Interfaces;

/// <summary>Tudo que é "social" (Fase 3): buscar pessoas, seguir, ver o
/// perfil público de outra conta. Separado de IPerfilService de propósito —
/// aquele é sobre editar a própria conta; este é sobre a relação entre
/// contas, um conjunto de regras bem diferente.</summary>
public interface IPerfilSocialService
{
    /// <summary>Busca por trecho do nome de usuário, excluindo o próprio
    /// solicitante do resultado (não faz sentido "seguir a si mesmo").</summary>
    Task<IReadOnlyList<UsuarioPublicoDto>> BuscarUsuariosAsync(Guid solicitanteId, string trecho);

    Task SeguirAsync(Guid solicitanteId, string nomeUsuarioAlvo);
    Task DeixarDeSeguirAsync(Guid solicitanteId, string nomeUsuarioAlvo);

    Task<PerfilPublicoDto> ObterPerfilPublicoAsync(Guid solicitanteId, string nomeUsuarioAlvo);

    Task<IReadOnlyList<UsuarioPublicoDto>> ListarSeguindoAsync(Guid usuarioId);
    Task<IReadOnlyList<UsuarioPublicoDto>> ListarSeguidoresAsync(Guid usuarioId);
}

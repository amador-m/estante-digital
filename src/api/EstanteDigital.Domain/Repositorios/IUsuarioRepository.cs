using EstanteDigital.Domain.Entidades;

namespace EstanteDigital.Domain.Repositorios;

public interface IUsuarioRepository
{
    Task<Usuario?> BuscarPorEmailAsync(string email);
    Task<Usuario?> BuscarPorIdAsync(Guid id);
    Task<Usuario?> BuscarPorNomeUsuarioAsync(string nomeUsuario);
    Task<Usuario> AdicionarAsync(Usuario usuario);
    Task<Usuario> AtualizarAsync(Usuario usuario);

    /// <summary>Busca por trecho do nome de usuário (case-insensitive, "contém"),
    /// pra tela de "buscar pessoas" — usada só pra achar quem seguir, nunca
    /// devolve e-mail nem senha (ver PerfilSocialService/UsuarioPublicoDto).
    /// Limitada a um número razoável de resultados (ver implementação).</summary>
    Task<IReadOnlyList<Usuario>> BuscarPorNomeParcialAsync(string trecho);
}

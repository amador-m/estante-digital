using EstanteDigital.Domain.Entidades;

namespace EstanteDigital.Domain.Repositorios;

/// <summary>
/// Contrato do repositório — o Domain só sabe que "existe alguém que guarda
/// e devolve Livros"; não sabe (nem precisa saber) se por trás disso tem um
/// banco de dados relacional, um arquivo JSON, ou uma lista em memória. Isso
/// é o que permite trocar a implementação (Infrastructure) sem tocar em
/// Domain/Application — a ideia central de Clean Architecture.
/// </summary>
/// <summary>
/// Todo método recebe o Guid do dono (usuarioId) e nunca devolve/altera um
/// Livro de outra pessoa — mesmo um BuscarPorIdAsync com um id válido
/// devolve null se o livro existir mas pertencer a outro usuário, pois a
/// consulta já nasce filtrada por usuarioId, em vez de buscar por Id e
/// comparar depois (assim nenhuma implementação consegue "esquecer" o
/// filtro de dono por engano).
/// </summary>
public interface ILivroRepository
{
    Task<IReadOnlyList<Livro>> ListarTodosAsync(Guid usuarioId);
    Task<Livro?> BuscarPorIdAsync(Guid id, Guid usuarioId);
    Task<Livro> AdicionarAsync(Livro livro);
    Task<Livro?> AtualizarAsync(Livro livro);
    Task<bool> RemoverAsync(Guid id, Guid usuarioId);
}

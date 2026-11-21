using EstanteDigital.Domain.Entidades;

namespace EstanteDigital.Domain.Repositorios;

public interface IDesafioRepository
{
    Task<Desafio> CriarAsync(Desafio desafio);
    Task<Desafio?> BuscarPorIdAsync(Guid id);

    /// <summary>Todos os desafios existentes (participação é aberta — não
    /// há "desafios privados" nesta versão, ver DesafioService).</summary>
    Task<IReadOnlyList<Desafio>> ListarTodosAsync();

    /// <summary>Só o criador pode excluir (ver DesafioService) — remove
    /// também as participações associadas, pra não deixar registros órfãos.</summary>
    Task<bool> RemoverAsync(Guid id, Guid criadorId);
}

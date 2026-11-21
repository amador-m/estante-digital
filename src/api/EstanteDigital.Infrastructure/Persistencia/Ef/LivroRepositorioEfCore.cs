#if USAR_POSTGRES
using EstanteDigital.Domain.Entidades;
using EstanteDigital.Domain.Repositorios;
using Microsoft.EntityFrameworkCore;

namespace EstanteDigital.Infrastructure.Persistencia.Ef;

/// <summary>
/// Implementação real (PostgreSQL via EF Core) de ILivroRepository — ver o
/// comentário em EstanteDigitalDbContext sobre por que este arquivo só
/// compila com `-p:UsarPostgres=true` e não pôde ser testado por mim.
/// Mesmo contrato e mesma regra de "nasce filtrado por usuarioId" que
/// ArquivoJsonLivroRepository já seguia — só troca onde os dados moram.
/// </summary>
public class LivroRepositorioEfCore : ILivroRepository
{
    private readonly EstanteDigitalDbContext _db;

    public LivroRepositorioEfCore(EstanteDigitalDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Livro>> ListarTodosAsync(Guid usuarioId) =>
        await _db.Livros.AsNoTracking()
            .Where(l => l.UsuarioId == usuarioId)
            .ToListAsync();

    public async Task<Livro?> BuscarPorIdAsync(Guid id, Guid usuarioId) =>
        await _db.Livros.AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id && l.UsuarioId == usuarioId);

    public async Task<Livro> AdicionarAsync(Livro livro)
    {
        _db.Livros.Add(livro);
        await _db.SaveChangesAsync();
        return livro;
    }

    public async Task<Livro?> AtualizarAsync(Livro livro)
    {
        // O livro que chega aqui já veio de um BuscarPorIdAsync(id, usuarioId)
        // feito pelo LivroService, então já foi confirmado como dono —
        // aqui só persiste o que já foi validado como dele.
        _db.Livros.Update(livro);
        await _db.SaveChangesAsync();
        return livro;
    }

    public async Task<bool> RemoverAsync(Guid id, Guid usuarioId)
    {
        var livro = await _db.Livros.FirstOrDefaultAsync(l => l.Id == id && l.UsuarioId == usuarioId);
        if (livro is null)
        {
            return false;
        }

        _db.Livros.Remove(livro);
        await _db.SaveChangesAsync();
        return true;
    }
}
#endif

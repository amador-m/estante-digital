#if USAR_POSTGRES
using EstanteDigital.Domain.Entidades;
using EstanteDigital.Domain.Repositorios;
using Microsoft.EntityFrameworkCore;

namespace EstanteDigital.Infrastructure.Persistencia.Ef;

/// <summary>Ver o comentário em EstanteDigitalDbContext — mesma observação
/// sobre compilação condicional e verificação se aplica aqui.</summary>
public class DesafioRepositorioEfCore : IDesafioRepository
{
    private readonly EstanteDigitalDbContext _db;

    public DesafioRepositorioEfCore(EstanteDigitalDbContext db)
    {
        _db = db;
    }

    public async Task<Desafio> CriarAsync(Desafio desafio)
    {
        _db.Desafios.Add(desafio);
        await _db.SaveChangesAsync();
        return desafio;
    }

    public async Task<Desafio?> BuscarPorIdAsync(Guid id) =>
        await _db.Desafios.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);

    public async Task<IReadOnlyList<Desafio>> ListarTodosAsync() =>
        await _db.Desafios.AsNoTracking().ToListAsync();

    public async Task<bool> RemoverAsync(Guid id, Guid criadorId)
    {
        var existente = await _db.Desafios.FirstOrDefaultAsync(d => d.Id == id && d.CriadorId == criadorId);
        if (existente is null)
        {
            return false;
        }

        _db.Desafios.Remove(existente);
        await _db.SaveChangesAsync();
        return true;
    }
}
#endif

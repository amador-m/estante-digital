#if USAR_POSTGRES
using EstanteDigital.Domain.Entidades;
using EstanteDigital.Domain.Repositorios;
using Microsoft.EntityFrameworkCore;

namespace EstanteDigital.Infrastructure.Persistencia.Ef;

/// <summary>Ver o comentário em EstanteDigitalDbContext — mesma observação
/// sobre compilação condicional e verificação se aplica aqui.</summary>
public class SeguidorRepositorioEfCore : ISeguidorRepository
{
    private readonly EstanteDigitalDbContext _db;

    public SeguidorRepositorioEfCore(EstanteDigitalDbContext db)
    {
        _db = db;
    }

    public async Task<Seguidor?> SeguirAsync(Guid seguidorId, Guid seguidoId)
    {
        var existente = await _db.Seguidores
            .FirstOrDefaultAsync(s => s.SeguidorId == seguidorId && s.SeguidoId == seguidoId);
        if (existente is not null)
        {
            return existente;
        }

        var nova = new Seguidor { Id = Guid.NewGuid(), SeguidorId = seguidorId, SeguidoId = seguidoId, CriadoEm = DateTime.UtcNow };
        _db.Seguidores.Add(nova);
        await _db.SaveChangesAsync();
        return nova;
    }

    public async Task DeixarDeSeguirAsync(Guid seguidorId, Guid seguidoId)
    {
        var existente = await _db.Seguidores
            .FirstOrDefaultAsync(s => s.SeguidorId == seguidorId && s.SeguidoId == seguidoId);
        if (existente is not null)
        {
            _db.Seguidores.Remove(existente);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<bool> EstaSeguindoAsync(Guid seguidorId, Guid seguidoId) =>
        await _db.Seguidores.AsNoTracking().AnyAsync(s => s.SeguidorId == seguidorId && s.SeguidoId == seguidoId);

    public async Task<int> ContarSeguidoresAsync(Guid usuarioId) =>
        await _db.Seguidores.AsNoTracking().CountAsync(s => s.SeguidoId == usuarioId);

    public async Task<int> ContarSeguindoAsync(Guid usuarioId) =>
        await _db.Seguidores.AsNoTracking().CountAsync(s => s.SeguidorId == usuarioId);

    public async Task<IReadOnlyList<Guid>> ListarSeguindoIdsAsync(Guid usuarioId) =>
        await _db.Seguidores.AsNoTracking()
            .Where(s => s.SeguidorId == usuarioId)
            .Select(s => s.SeguidoId)
            .ToListAsync();

    public async Task<IReadOnlyList<Guid>> ListarSeguidoresIdsAsync(Guid usuarioId) =>
        await _db.Seguidores.AsNoTracking()
            .Where(s => s.SeguidoId == usuarioId)
            .Select(s => s.SeguidorId)
            .ToListAsync();
}
#endif

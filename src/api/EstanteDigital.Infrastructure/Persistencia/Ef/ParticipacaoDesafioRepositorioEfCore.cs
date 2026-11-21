#if USAR_POSTGRES
using EstanteDigital.Domain.Entidades;
using EstanteDigital.Domain.Repositorios;
using Microsoft.EntityFrameworkCore;

namespace EstanteDigital.Infrastructure.Persistencia.Ef;

public class ParticipacaoDesafioRepositorioEfCore : IParticipacaoDesafioRepository
{
    private readonly EstanteDigitalDbContext _db;

    public ParticipacaoDesafioRepositorioEfCore(EstanteDigitalDbContext db)
    {
        _db = db;
    }

    public async Task<ParticipacaoDesafio> EntrarAsync(Guid desafioId, Guid usuarioId)
    {
        var existente = await _db.ParticipacoesDesafio
            .FirstOrDefaultAsync(p => p.DesafioId == desafioId && p.UsuarioId == usuarioId);
        if (existente is not null)
        {
            return existente;
        }

        var nova = new ParticipacaoDesafio { Id = Guid.NewGuid(), DesafioId = desafioId, UsuarioId = usuarioId, EntrouEm = DateTime.UtcNow };
        _db.ParticipacoesDesafio.Add(nova);
        await _db.SaveChangesAsync();
        return nova;
    }

    public async Task SairAsync(Guid desafioId, Guid usuarioId)
    {
        var existente = await _db.ParticipacoesDesafio
            .FirstOrDefaultAsync(p => p.DesafioId == desafioId && p.UsuarioId == usuarioId);
        if (existente is not null)
        {
            _db.ParticipacoesDesafio.Remove(existente);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<bool> ParticipaAsync(Guid desafioId, Guid usuarioId) =>
        await _db.ParticipacoesDesafio.AsNoTracking().AnyAsync(p => p.DesafioId == desafioId && p.UsuarioId == usuarioId);

    public async Task<IReadOnlyList<Guid>> ListarParticipantesIdsAsync(Guid desafioId) =>
        await _db.ParticipacoesDesafio.AsNoTracking()
            .Where(p => p.DesafioId == desafioId)
            .Select(p => p.UsuarioId)
            .ToListAsync();

    public async Task<IReadOnlyList<Guid>> ListarDesafioIdsDoUsuarioAsync(Guid usuarioId) =>
        await _db.ParticipacoesDesafio.AsNoTracking()
            .Where(p => p.UsuarioId == usuarioId)
            .Select(p => p.DesafioId)
            .ToListAsync();

    public async Task RemoverTodasDoDesafioAsync(Guid desafioId)
    {
        var participacoes = await _db.ParticipacoesDesafio.Where(p => p.DesafioId == desafioId).ToListAsync();
        if (participacoes.Count > 0)
        {
            _db.ParticipacoesDesafio.RemoveRange(participacoes);
            await _db.SaveChangesAsync();
        }
    }
}
#endif

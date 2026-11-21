#if USAR_POSTGRES
using EstanteDigital.Domain.Entidades;
using EstanteDigital.Domain.Repositorios;
using Microsoft.EntityFrameworkCore;

namespace EstanteDigital.Infrastructure.Persistencia.Ef;

public class AtividadeRepositorioEfCore : IAtividadeRepository
{
    private readonly EstanteDigitalDbContext _db;

    public AtividadeRepositorioEfCore(EstanteDigitalDbContext db)
    {
        _db = db;
    }

    public async Task<Atividade> RegistrarAsync(Atividade atividade)
    {
        _db.Atividades.Add(atividade);
        await _db.SaveChangesAsync();
        return atividade;
    }

    public async Task<IReadOnlyList<Atividade>> ListarDeUsuariosAsync(IReadOnlyList<Guid> usuarioIds, int limite) =>
        await _db.Atividades.AsNoTracking()
            .Where(a => usuarioIds.Contains(a.UsuarioId))
            .OrderByDescending(a => a.CriadoEm)
            .Take(limite)
            .ToListAsync();
}
#endif

#if USAR_POSTGRES
using EstanteDigital.Domain.Entidades;
using EstanteDigital.Domain.Repositorios;
using Microsoft.EntityFrameworkCore;

namespace EstanteDigital.Infrastructure.Persistencia.Ef;

public class CheckInRepositorioEfCore : ICheckInRepository
{
    private readonly EstanteDigitalDbContext _db;

    public CheckInRepositorioEfCore(EstanteDigitalDbContext db)
    {
        _db = db;
    }

    public async Task<CheckInLeitura> RegistrarAsync(Guid usuarioId, DateTime data)
    {
        var dataNormalizada = data.Date;
        var existente = await _db.CheckInsLeitura
            .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId && c.Data == dataNormalizada);
        if (existente is not null)
        {
            return existente;
        }

        var novo = new CheckInLeitura { Id = Guid.NewGuid(), UsuarioId = usuarioId, Data = dataNormalizada, CriadoEm = DateTime.UtcNow };
        _db.CheckInsLeitura.Add(novo);
        await _db.SaveChangesAsync();
        return novo;
    }

    public async Task<bool> JaRegistrouAsync(Guid usuarioId, DateTime data) =>
        await _db.CheckInsLeitura.AsNoTracking().AnyAsync(c => c.UsuarioId == usuarioId && c.Data == data.Date);

    public async Task<IReadOnlyList<DateTime>> ListarDatasAsync(Guid usuarioId) =>
        await _db.CheckInsLeitura.AsNoTracking()
            .Where(c => c.UsuarioId == usuarioId)
            .Select(c => c.Data)
            .ToListAsync();
}
#endif

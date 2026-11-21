#if USAR_POSTGRES
using EstanteDigital.Domain.Entidades;
using EstanteDigital.Domain.Repositorios;
using Microsoft.EntityFrameworkCore;

namespace EstanteDigital.Infrastructure.Persistencia.Ef;

/// <summary>Ver o comentário em EstanteDigitalDbContext — mesma observação
/// sobre compilação condicional e verificação se aplica aqui.</summary>
public class UsuarioRepositorioEfCore : IUsuarioRepository
{
    private readonly EstanteDigitalDbContext _db;

    public UsuarioRepositorioEfCore(EstanteDigitalDbContext db)
    {
        _db = db;
    }

    public async Task<Usuario?> BuscarPorEmailAsync(string email) =>
        await _db.Usuarios.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

    public async Task<Usuario?> BuscarPorIdAsync(Guid id) =>
        await _db.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);

    public async Task<Usuario?> BuscarPorNomeUsuarioAsync(string nomeUsuario) =>
        await _db.Usuarios.AsNoTracking()
            .FirstOrDefaultAsync(u => u.NomeUsuario.ToLower() == nomeUsuario.ToLower());

    public async Task<IReadOnlyList<Usuario>> BuscarPorNomeParcialAsync(string trecho) =>
        await _db.Usuarios.AsNoTracking()
            .Where(u => EF.Functions.ILike(u.NomeUsuario, $"%{trecho}%"))
            .OrderBy(u => u.NomeUsuario)
            .ToListAsync();

    public async Task<Usuario> AdicionarAsync(Usuario usuario)
    {
        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();
        return usuario;
    }

    public async Task<Usuario> AtualizarAsync(Usuario usuario)
    {
        _db.Usuarios.Update(usuario);
        await _db.SaveChangesAsync();
        return usuario;
    }
}
#endif

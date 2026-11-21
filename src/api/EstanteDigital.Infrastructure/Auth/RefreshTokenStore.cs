using System.Collections.Concurrent;

namespace EstanteDigital.Infrastructure.Auth;

public record RefreshTokenInfo(Guid UsuarioId, DateTime ExpiraEm);

/// <summary>
/// Guarda os refresh tokens em memória (zera ao reiniciar o container —
/// aceitável aqui porque o pior caso é só "precisar logar de novo", nada
/// de dados perdidos; quem precisa sobreviver a reinícios é o catálogo de
/// livros e os usuários cadastrados, que ficam persistidos em
/// Persistencia/).
///
/// Chaveado por Guid do usuário (não por e-mail) — e-mail pode, em teoria,
/// ser reaproveitado ou alterado; o id é a identidade estável do dono da
/// estante.
/// </summary>
public class RefreshTokenStore
{
    private readonly ConcurrentDictionary<string, RefreshTokenInfo> _tokens = new();
    private static readonly TimeSpan Validade = TimeSpan.FromDays(7);

    public string Gerar(Guid usuarioId)
    {
        var token = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32))
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');

        _tokens[token] = new RefreshTokenInfo(usuarioId, DateTime.UtcNow.Add(Validade));
        return token;
    }

    public Guid? ValidarEUsar(string token)
    {
        if (!_tokens.TryGetValue(token, out var info))
        {
            return null;
        }

        // Rotação: o token usado é descartado, mesmo se ainda estiver válido.
        _tokens.TryRemove(token, out _);

        if (info.ExpiraEm < DateTime.UtcNow)
        {
            return null;
        }

        return info.UsuarioId;
    }

    public void Revogar(string token) => _tokens.TryRemove(token, out _);
}

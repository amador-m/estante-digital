using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace EstanteDigital.Infrastructure.Auth;

/// <summary>
/// JWT (RFC 7519) montado manualmente com HMAC-SHA256 — a mesma estratégia
/// das Aulas 12/16: os pacotes oficiais (System.IdentityModel.Tokens.Jwt /
/// Microsoft.AspNetCore.Authentication.JwtBearer) dependem do NuGet, que
/// fica bloqueado neste ambiente de sandbox. O token gerado aqui é um JWT
/// de verdade (decodificável em jwt.io, assinatura verificável), só que
/// sem depender de pacote externo nenhum.
/// </summary>
public class TokenService
{
    private readonly JwtOptions _opcoes;

    public TokenService(JwtOptions opcoes)
    {
        _opcoes = opcoes;
    }

    public string GerarAccessToken(Guid usuarioId, string email)
    {
        var cabecalho = new { alg = "HS256", typ = "JWT" };
        var agora = DateTimeOffset.UtcNow;
        var expiracao = agora.AddMinutes(_opcoes.ExpireMinutes);

        var payload = new Dictionary<string, object>
        {
            // "sub" (subject) é o id do usuário, não o e-mail — é o que
            // identifica a estante dele de forma estável mesmo que um dia
            // ele troque de e-mail. O e-mail vai como claim separada, só
            // para exibição (ex.: "Olá, maria@..." na barra superior).
            ["sub"] = usuarioId.ToString(),
            ["email"] = email,
            ["iss"] = _opcoes.Issuer,
            ["aud"] = _opcoes.Audience,
            ["iat"] = agora.ToUnixTimeSeconds(),
            ["exp"] = expiracao.ToUnixTimeSeconds()
        };

        var cabecalhoCodificado = CodificarBase64Url(JsonSerializer.SerializeToUtf8Bytes(cabecalho));
        var payloadCodificado = CodificarBase64Url(JsonSerializer.SerializeToUtf8Bytes(payload));
        var dadosAssinados = $"{cabecalhoCodificado}.{payloadCodificado}";

        var assinatura = AssinarHmac(dadosAssinados);
        return $"{dadosAssinados}.{assinatura}";
    }

    /// <summary>Valida assinatura e expiração. Retorna os claims se válido, ou null caso contrário.</summary>
    public IReadOnlyDictionary<string, object>? ValidarToken(string token)
    {
        var partes = token.Split('.');
        if (partes.Length != 3)
        {
            return null;
        }

        var dadosAssinados = $"{partes[0]}.{partes[1]}";
        var assinaturaEsperada = AssinarHmac(dadosAssinados);

        var assinaturaRecebida = Encoding.UTF8.GetBytes(partes[2]);
        var assinaturaCalculada = Encoding.UTF8.GetBytes(assinaturaEsperada);

        // Comparação em tempo constante — evita um ataque de timing na checagem da assinatura.
        if (assinaturaRecebida.Length != assinaturaCalculada.Length ||
            !CryptographicOperations.FixedTimeEquals(assinaturaRecebida, assinaturaCalculada))
        {
            return null;
        }

        try
        {
            var payloadJson = DecodificarBase64Url(partes[1]);
            var payload = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(payloadJson);
            if (payload is null)
            {
                return null;
            }

            var exp = payload["exp"].GetInt64();
            if (DateTimeOffset.FromUnixTimeSeconds(exp) < DateTimeOffset.UtcNow)
            {
                return null;
            }

            return payload.ToDictionary(kv => kv.Key, kv => (object)kv.Value);
        }
        catch
        {
            return null;
        }
    }

    private string AssinarHmac(string dados)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_opcoes.Key));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(dados));
        return CodificarBase64Url(hash);
    }

    private static string CodificarBase64Url(byte[] dados) =>
        Convert.ToBase64String(dados).Replace('+', '-').Replace('/', '_').TrimEnd('=');

    private static byte[] DecodificarBase64Url(string texto)
    {
        var s = texto.Replace('-', '+').Replace('_', '/');
        switch (s.Length % 4)
        {
            case 2: s += "=="; break;
            case 3: s += "="; break;
        }
        return Convert.FromBase64String(s);
    }
}

using System.Security.Cryptography;

namespace EstanteDigital.Infrastructure.Auth;

/// <summary>
/// Hash de senha com PBKDF2-HMACSHA256 (RFC 2898), usando só
/// System.Security.Cryptography — nenhum pacote NuGet (nem BCrypt.Net, nem
/// o ASP.NET Core Identity) é necessário, mesma razão de sempre neste
/// projeto: o sandbox onde eu trabalho bloqueia o NuGet. É literalmente o
/// mesmo algoritmo que o PasswordHasher do ASP.NET Core Identity usa por
/// baixo dos panos, só escrito à mão.
///
/// Formato armazenado (uma única string, para caber numa coluna de texto):
///   {iterações}.{salt em Base64}.{hash em Base64}
/// Guardar as iterações junto permite aumentar esse número no futuro sem
/// invalidar senhas já cadastradas com um valor mais baixo.
/// </summary>
public static class SenhaHasher
{
    private const int TamanhoSaltBytes = 16;
    private const int TamanhoHashBytes = 32;
    private const int Iteracoes = 100_000;

    public static string Hash(string senha)
    {
        var salt = RandomNumberGenerator.GetBytes(TamanhoSaltBytes);
        var hash = Rfc2898DeriveBytes.Pbkdf2(senha, salt, Iteracoes, HashAlgorithmName.SHA256, TamanhoHashBytes);

        return $"{Iteracoes}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public static bool Verificar(string senha, string hashArmazenado)
    {
        var partes = hashArmazenado.Split('.');
        if (partes.Length != 3 || !int.TryParse(partes[0], out var iteracoes))
        {
            return false;
        }

        var salt = Convert.FromBase64String(partes[1]);
        var hashEsperado = Convert.FromBase64String(partes[2]);

        var hashCalculado = Rfc2898DeriveBytes.Pbkdf2(senha, salt, iteracoes, HashAlgorithmName.SHA256, hashEsperado.Length);

        // Comparação em tempo constante — mesmo cuidado já usado na
        // verificação da assinatura do JWT em TokenService.
        return CryptographicOperations.FixedTimeEquals(hashCalculado, hashEsperado);
    }
}

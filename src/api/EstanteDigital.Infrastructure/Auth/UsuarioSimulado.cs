namespace EstanteDigital.Infrastructure.Auth;

public record UsuarioSimulado(string Email, string Senha, string Role);

/// <summary>
/// Este é um projeto de portfólio/estudo com UM único dono do catálogo
/// (você). Por isso existe apenas uma conta simulada, com papel "Admin" —
/// diferente da Aula 16, onde fazia sentido ter usuário comum + admin
/// porque a listagem também era protegida. Aqui a listagem é pública, então
/// só quem precisa logar é quem vai gerenciar o acervo.
/// </summary>
public static class UsuariosSimulados
{
    private static readonly List<UsuarioSimulado> Usuarios = new()
    {
        new UsuarioSimulado("admin@estantedigital.com", "123456", "Admin")
    };

    public static UsuarioSimulado? Validar(string email, string senha) =>
        Usuarios.FirstOrDefault(u =>
            string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase) &&
            u.Senha == senha);

    /// <summary>
    /// Usado no fluxo de refresh: a identidade (email) já veio validada pelo
    /// RefreshTokenStore, então aqui só precisamos achar o papel (role) dela
    /// sem conferir senha de novo.
    /// </summary>
    public static UsuarioSimulado? BuscarPorEmail(string email) =>
        Usuarios.FirstOrDefault(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));
}

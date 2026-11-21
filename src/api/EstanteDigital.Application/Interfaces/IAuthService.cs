using EstanteDigital.Application.DTOs;

namespace EstanteDigital.Application.Interfaces;

/// <summary>
/// Fica em Application (não em Infrastructure) porque é o "caso de uso"
/// de autenticação do ponto de vista do sistema — o controller só conhece
/// esta interface; quem efetivamente gera o token, confere a senha e guarda
/// o refresh token (TokenService/UsuariosSimulados/RefreshTokenStore) é
/// detalhe de Infrastructure, plugado por injeção de dependência.
/// </summary>
public interface IAuthService
{
    /// <summary>Cria a conta (com hash de senha) e já devolve os tokens —
    /// não faz sentido pedir pra pessoa logar de novo logo após se cadastrar.
    /// Lança ValidacaoException se o e-mail já estiver em uso ou os dados
    /// forem inválidos.</summary>
    Task<TokenResponseDto> RegistrarAsync(RegistroDto dto);
    Task<TokenResponseDto?> LoginAsync(LoginDto dto);
    Task<TokenResponseDto?> RefreshAsync(string refreshToken);
    Task LogoutAsync(string refreshToken);
}

namespace EstanteDigital.Application.DTOs;

public record LoginDto(string Email, string Senha);

/// <summary>
/// Sem "Role" (diferente das Aulas 12/16 e da primeira versão deste
/// projeto): aqui todo usuário autenticado é igual — gerencia só a própria
/// estante, ninguém tem papel especial. O e-mail para exibição na tela já
/// vem decodificado do próprio token no front (ver services/jwt.js).
/// </summary>
public record TokenResponseDto(string AccessToken, string RefreshToken);

public record RefreshDto(string RefreshToken);

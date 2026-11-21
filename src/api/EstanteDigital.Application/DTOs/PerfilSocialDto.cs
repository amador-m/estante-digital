namespace EstanteDigital.Application.DTOs;

/// <summary>
/// Versão "pública" de um usuário — usada em listas (resultado de busca,
/// "quem eu sigo", "meus seguidores"). Nunca inclui e-mail nem qualquer
/// coisa que vá além do que o próprio Perfil já expõe de propósito ao
/// resto do mundo (nome de usuário, ícone, cor).
/// </summary>
public record UsuarioPublicoDto(
    Guid Id,
    string NomeUsuario,
    string Icone,
    string CorPrincipal
);

/// <summary>
/// Perfil "de fora" — o que aparece quando alguém abre a página de outra
/// conta. Identidade e estatísticas são sempre visíveis (servem pra decidir
/// se vale seguir); <see cref="Estante"/> só vem preenchida quando
/// <see cref="VoceSegue"/> ou <see cref="EhVoceMesmo"/> forem verdadeiros —
/// ver decisão de visibilidade no LEIA-ME (Fase 3): seguir dá acesso à
/// estante inteira, read-only, incluindo resenhas.
/// </summary>
public record PerfilPublicoDto(
    Guid Id,
    string NomeUsuario,
    string Icone,
    string CorPrincipal,
    DateTime CriadoEm,
    int? MetaLeituraAnual,
    int LivrosLidosNoAnoAtual,
    int TotalSeguidores,
    int TotalSeguindo,
    bool VoceSegue,
    bool EhVoceMesmo,
    IReadOnlyList<LivroDto> Estante
);

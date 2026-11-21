namespace EstanteDigital.Application.DTOs;

/// <summary>
/// Uma linha do feed de atividade. Os campos opcionais variam conforme
/// Tipo — o front decide o que renderizar olhando pra Tipo, igual decide
/// o que mostrar num LivroDto olhando pra StatusLeitura. Os tipos possíveis
/// (espelhando TipoAtividade no Domain): "TerminouLivro", "ResenhaPublicada",
/// "MetaBatida", "CheckIn", "DesafioConcluido".
/// </summary>
public record AtividadeDto(
    Guid Id,
    Guid UsuarioId,
    string NomeUsuario,
    string Icone,
    string CorPrincipal,
    string Tipo,
    DateTime CriadoEm,
    Guid? LivroId,
    string? LivroTitulo,
    int? MetaValor,
    int? SequenciaDias,
    Guid? DesafioId,
    string? DesafioTitulo,
    int? LivrosNoDesafio
);

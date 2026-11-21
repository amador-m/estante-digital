namespace EstanteDigital.Application.DTOs;

/// <summary>
/// Entrada do POST. Genero/StatusLeitura chegam como string (o jeito mais
/// simples de trafegar um enum em JSON/JS) e são convertidos e validados
/// dentro do LivroService.
/// </summary>
public record CriarLivroDto(
    string Titulo,
    string Autor,
    string? Serie,
    string Genero,
    int AnoPublicacao,
    string SinopseCurta,
    int? NumeroPaginas,
    string? StatusLeitura,
    int? Avaliacao,
    int? PaginaAtual,
    string? Resenha
);

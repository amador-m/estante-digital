namespace EstanteDigital.Application.DTOs;

/// <summary>Entrada do PUT — mesmos campos do CriarLivroDto (todo o registro é reenviado).</summary>
public record AtualizarLivroDto(
    string Titulo,
    string Autor,
    string? Serie,
    string Genero,
    int AnoPublicacao,
    string SinopseCurta,
    int? NumeroPaginas,
    string StatusLeitura,
    int? Avaliacao,
    int? PaginaAtual,
    string? Resenha
);

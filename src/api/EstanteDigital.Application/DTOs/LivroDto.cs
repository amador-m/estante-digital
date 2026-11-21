namespace EstanteDigital.Application.DTOs;

/// <summary>
/// O que a API devolve para fora. Existe separado da entidade Livro (Domain)
/// de propósito: se amanhã o Domain ganhar um campo interno (ex.: um custo
/// de aquisição, uma nota privada), não vaza pra API sem que alguém decida
/// explicitamente colocar esse campo aqui também.
/// </summary>
public record LivroDto(
    Guid Id,
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
    string? Resenha,
    DateTime? DataConclusao,
    DateTime CriadoEm,
    DateTime AtualizadoEm
);

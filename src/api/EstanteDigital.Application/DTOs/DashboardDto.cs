namespace EstanteDigital.Application.DTOs;

/// <summary>
/// Agregados prontos para a tela de Dashboard do front — calculados aqui
/// (Application) e não no React, pra não duplicar essa lógica se amanhã
/// existir um segundo cliente (um app mobile, por exemplo) consumindo a
/// mesma API.
/// </summary>
public record DashboardDto(
    int TotalDeLivros,
    int TotalLidos,
    int TotalLendo,
    int TotalQueroLer,
    double? MediaAvaliacaoLidos,
    IReadOnlyList<ContagemPorChaveDto> PorGenero,
    IReadOnlyList<ContagemPorChaveDto> PorAutor,
    LivroDto? UltimoAdicionado,
    int? MetaLeituraAnual,
    int LivrosLidosNoAnoAtual
);

public record ContagemPorChaveDto(string Chave, int Quantidade);

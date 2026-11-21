namespace EstanteDigital.Domain.Enums;

/// <summary>
/// Status de leitura do livro na estante — é o que dá vida ao dashboard
/// (quantos lidos, quantos na fila, etc.), diferente de um catálogo de
/// produtos comum que só teria "em estoque" ou não.
/// </summary>
public enum StatusLeitura
{
    QueroLer = 0,
    Lendo = 1,
    Lido = 2
}

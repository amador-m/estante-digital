namespace EstanteDigital.Domain.Enums;

/// <summary>
/// Gênero literário do livro. Enum simples (em vez de string livre) para
/// garantir que o front sempre receba um conjunto fechado de opções nos
/// filtros e no formulário de cadastro.
/// </summary>
public enum GeneroLivro
{
    Fantasia = 0,
    Suspense = 1,
    Romance = 2,
    Drama = 3,
    Aventura = 4,
    Biografia = 5,
    Outro = 6
}

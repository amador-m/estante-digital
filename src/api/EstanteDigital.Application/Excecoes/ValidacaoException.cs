namespace EstanteDigital.Application.Excecoes;

/// <summary>
/// Erro de regra de negócio (não de infraestrutura). O controller captura
/// isso e devolve 400 com a mensagem — é a mesma estratégia usada nas
/// aulas anteriores de API, só que agora centralizada numa exceção própria
/// em vez de checagens soltas no controller.
/// </summary>
public class ValidacaoException : Exception
{
    public ValidacaoException(string mensagem) : base(mensagem) { }
}

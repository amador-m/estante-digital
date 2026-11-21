namespace EstanteDigital.Application.Constantes;

/// <summary>
/// Conjunto fixo de ícones e cores que uma conta pode escolher pro próprio
/// perfil — nenhuma das duas listas aceita valor livre (nem upload de
/// imagem, nem hex arbitrário): a pessoa escolhe de um menu pequeno,
/// garantindo que tudo renderiza bem nos dois temas.
///
/// Front-end: estas mesmas chaves (e só estas) estão espelhadas em
/// `src/frontend/src/constantes/perfil.js`. Se mudar uma lista aqui, mudar
/// a outra também — não existe um endpoint que sincroniza as duas automaticamente
/// (decisão consciente pra não adicionar mais uma chamada de rede só pra isso).
/// </summary>
public static class OpcoesPerfil
{
    public static readonly IReadOnlyList<string> Icones = new[]
    {
        "livro-aberto",
        "pilha-livros",
        "marca-pagina",
        "coruja",
        "pena",
        "oculos",
        "xicara",
        "estrela",
    };

    public static readonly IReadOnlyList<string> Cores = new[]
    {
        "azul",
        "roxo",
        "rosa",
        "vermelho",
        "laranja",
        "verde",
        "teal",
        "grafite",
    };

    public const string IconePadrao = "livro-aberto";
    public const string CorPadrao = "azul";
}

using EstanteDigital.Domain.Enums;

namespace EstanteDigital.Domain.Entidades;

/// <summary>
/// Entidade central do domínio. Fica no projeto Domain porque não depende
/// de nada de fora (nem do ASP.NET Core, nem de como os dados são
/// persistidos) — é só a regra "o que é um Livro" dentro desta aplicação.
/// </summary>
public class Livro
{
    public Guid Id { get; set; }

    /// <summary>Dono da estante a que este livro pertence — nenhuma consulta de
    /// Livro deveria rodar sem filtrar por este campo (ver ILivroRepository).</summary>
    public Guid UsuarioId { get; set; }

    public string Titulo { get; set; } = string.Empty;
    public string Autor { get; set; } = string.Empty;

    /// <summary>Série/saga a que o livro pertence (ex.: "Trono de Vidro"). Nulo para livros avulsos.</summary>
    public string? Serie { get; set; }

    public GeneroLivro Genero { get; set; }
    public int AnoPublicacao { get; set; }
    public string SinopseCurta { get; set; } = string.Empty;

    /// <summary>Número de páginas é opcional — depende da edição, então não é obrigatório no cadastro.</summary>
    public int? NumeroPaginas { get; set; }

    public StatusLeitura StatusLeitura { get; set; } = StatusLeitura.QueroLer;

    /// <summary>Nota pessoal de 1 a 5. Só faz sentido quando StatusLeitura == Lido.</summary>
    public int? Avaliacao { get; set; }

    /// <summary>Página em que a pessoa está agora. Só faz sentido quando
    /// StatusLeitura == Lendo; usada pra mostrar uma barra de progresso
    /// quando NumeroPaginas também estiver preenchido (ver LivroService
    /// pra validação de que não passa de NumeroPaginas).</summary>
    public int? PaginaAtual { get; set; }

    /// <summary>Resenha em texto livre — diferente da Avaliacao (nota 1-5),
    /// é a parte "social" deste livro: quando a conta segue outra, é isto
    /// (junto com o resto da estante) que fica visível no perfil público
    /// (ver Fase 3 no LEIA-ME). Pode ser escrita a qualquer momento, não só
    /// quando Lido — não há essa trava de propósito, pra não impedir uma
    /// pessoa de ir anotando impressões enquanto ainda está lendo.</summary>
    public string? Resenha { get; set; }

    /// <summary>Data em que o livro foi marcado como Lido — usada só pra
    /// calcular o progresso da meta de leitura anual (quantos livros
    /// concluídos no ano corrente). Setada automaticamente pelo
    /// LivroService quando o status muda PARA Lido, e limpa quando muda
    /// PARA QUALQUER OUTRO status — não é um campo editável livremente
    /// pela pessoa, é derivado da mudança de status.</summary>
    public DateTime? DataConclusao { get; set; }

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;
}

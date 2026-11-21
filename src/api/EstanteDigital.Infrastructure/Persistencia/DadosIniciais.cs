using EstanteDigital.Domain.Entidades;
using EstanteDigital.Domain.Enums;

namespace EstanteDigital.Infrastructure.Persistencia;

/// <summary>
/// Catálogo inicial com livros e autores reais (pesquisados antes de
/// montar esta lista — título, autor, série e ano conferidos; as sinopses
/// são redações originais minhas sobre a premissa pública de cada livro,
/// não trechos copiados de orelha/contracapa). Mudanças feitas em relação
/// ao pedido original:
///
/// - "Rafael Montes" → corrigido para "Raphael Montes" (grafia real do
///   autor de Bom Dia, Verônica / Dias Perfeitos).
/// - "L.C. Almeida" → não encontrei nenhuma bibliografia confirmável pra
///   esse nome; você pediu pra trocar por outra autora internacional de
///   fantasia/suspense conhecida, e eu escolhi Leigh Bardugo (Grishaverse).
/// - Status de leitura e avaliações são dados de demonstração (pra dar
///   vida ao dashboard), não um retrato real da sua estante.
///
/// Fase 2 (contas reais / estante por usuário): como agora cada conta só
/// vê os próprios livros, este catálogo de demonstração passou a pertencer
/// a uma conta demo fixa (ver ObterUsuarioDemo) — assim, quem clona o
/// projeto e sobe ele do zero consegue logar e já ver uma estante
/// preenchida, em vez de uma tela vazia. Uma conta nova, cadastrada de
/// verdade, começa com a estante em branco — comportamento esperado de um
/// sistema multiusuário.
/// </summary>
public static class DadosIniciais
{
    /// <summary>Id fixo (não aleatório) só para a conta demo ser previsível
    /// entre uma subida do container e outra, enquanto o arquivo JSON ainda
    /// não existe.</summary>
    public static readonly Guid UsuarioDemoId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public const string EmailUsuarioDemo = "demo@estantedigital.com";
    public const string SenhaUsuarioDemo = "Estante@123";

    /// <summary>
    /// Fase 3 (seguir outras pessoas): sem uma segunda conta semeada, não
    /// dava pra demonstrar nem testar "seguir" sem cadastrar manualmente
    /// outra conta toda vez. Esta conta existe só pra isso — tem a própria
    /// estantezinha, pequena e com resenhas escritas, pra conta demo seguir
    /// e já ver o recurso funcionando de primeira.
    /// </summary>
    public static readonly Guid UsuarioAmigaLeitoraId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public const string EmailAmigaLeitora = "amiga@estantedigital.com";
    public const string SenhaAmigaLeitora = "Estante@123";

    public static IReadOnlyList<Usuario> ObterUsuarioDemo() => new List<Usuario>
    {
        new()
        {
            Id = UsuarioDemoId,
            Email = EmailUsuarioDemo,
            SenhaHash = EstanteDigital.Infrastructure.Auth.SenhaHasher.Hash(SenhaUsuarioDemo),
            NomeUsuario = "demo",
            Icone = EstanteDigital.Application.Constantes.OpcoesPerfil.IconePadrao,
            CorPrincipal = EstanteDigital.Application.Constantes.OpcoesPerfil.CorPadrao,
            MetaLeituraAnual = 20,
            CriadoEm = DateTime.UtcNow
        },
        new()
        {
            Id = UsuarioAmigaLeitoraId,
            Email = EmailAmigaLeitora,
            SenhaHash = EstanteDigital.Infrastructure.Auth.SenhaHasher.Hash(SenhaAmigaLeitora),
            NomeUsuario = "amigaleitora",
            Icone = "coruja",
            CorPrincipal = "roxo",
            MetaLeituraAnual = 12,
            CriadoEm = DateTime.UtcNow
        }
    };

    public static IReadOnlyList<Livro> ObterCatalogoInicial()
    {
        var agora = DateTime.UtcNow;
        // Placeholder — o carimbo real de cada livro é definido depois, na
        // segunda passada abaixo, para que o ÚLTIMO item desta lista (e não
        // o primeiro) seja o "mais recente" no dashboard — mais intuitivo
        // pra demonstração do que a ordem inversa.
        DateTime ProximoCarimbo() => agora;

        var livros = new List<Livro>
        {
            // --- Sarah J. Maas — Trono de Vidro (fantasia) ---
            NovoLivro("Trono de Vidro", "Sarah J. Maas", "Trono de Vidro", GeneroLivro.Fantasia, 2012,
                "Celaena Sardothien, uma assassina lendária presa nas minas de sal de Endovier, é tirada da prisão para competir como campeã do rei num torneio brutal. Se vencer, ganha a liberdade — mas a corte está cheia de perigos que vão muito além dos outros competidores.",
                StatusLeitura.Lido, 5, ProximoCarimbo()),
            NovoLivro("Coroa da Meia-Noite", "Sarah J. Maas", "Trono de Vidro", GeneroLivro.Fantasia, 2013,
                "Agora como Campeã do Rei, Celaena é forçada a executar as ordens da coroa enquanto esconde seus próprios planos. Segredos antigos sobre o reino começam a vir à tona, e ela precisa decidir de que lado realmente está.",
                StatusLeitura.Lido, 5, ProximoCarimbo()),
            NovoLivro("Herdeira do Fogo", "Sarah J. Maas", "Trono de Vidro", GeneroLivro.Fantasia, 2014,
                "Fugindo da corte após os eventos do livro anterior, Celaena parte em busca de respostas sobre seus poderes e sua verdadeira identidade, enquanto uma ameaça mágica antiga volta a se mover no reino.",
                StatusLeitura.Lido, 4, ProximoCarimbo()),
            NovoLivro("Rainha das Sombras", "Sarah J. Maas", "Trono de Vidro", GeneroLivro.Fantasia, 2015,
                "Celaena retorna à capital, não mais escondendo quem realmente é, disposta a enfrentar o rei de frente — as alianças se reorganizam enquanto a guerra se torna inevitável.",
                StatusLeitura.Lido, 5, ProximoCarimbo()),
            NovoLivro("Império de Tempestades", "Sarah J. Maas", "Trono de Vidro", GeneroLivro.Fantasia, 2016,
                "Com a guerra batendo à porta, personagens de reinos diferentes precisam se unir contra um inimigo comum que ameaça destruir tudo o que conhecem.",
                StatusLeitura.Lendo, null, ProximoCarimbo()),
            NovoLivro("Torre do Amanhecer", "Sarah J. Maas", "Trono de Vidro", GeneroLivro.Fantasia, 2017,
                "Enquanto a guerra se espalha pelos outros reinos, parte do grupo parte numa jornada separada em busca de cura e de aliados num território desconhecido.",
                StatusLeitura.QueroLer, null, ProximoCarimbo()),
            NovoLivro("Reino de Cinzas", "Sarah J. Maas", "Trono de Vidro", GeneroLivro.Fantasia, 2018,
                "O confronto final da saga: todos os fios da história se encontram numa batalha decisiva pelo destino do reino e de seu povo.",
                StatusLeitura.QueroLer, null, ProximoCarimbo()),

            // --- Raphael Montes (suspense/thriller) ---
            NovoLivro("Suicidas", "Raphael Montes", null, GeneroLivro.Suspense, 2013,
                "Um grupo de jovens entra para um pacto que promete liberdade através da morte. A história acompanha o que leva cada um deles a essa decisão e as consequências que ultrapassam o que haviam planejado.",
                StatusLeitura.Lido, 4, ProximoCarimbo()),
            NovoLivro("Jantar Secreto", "Raphael Montes", null, GeneroLivro.Suspense, 2013,
                "Durante um jantar entre amigos, um jogo de confissões vira pretexto para expor segredos que nenhum dos convidados estava pronto para revelar.",
                StatusLeitura.QueroLer, null, ProximoCarimbo()),
            NovoLivro("Dias Perfeitos", "Raphael Montes", null, GeneroLivro.Suspense, 2014,
                "Um estudante de medicina obcecado por uma colega a sequestra, convencido de que pode fazer com que ela se apaixone por ele — um thriller psicológico sobre obsessão e controle.",
                StatusLeitura.Lido, 5, ProximoCarimbo()),
            NovoLivro("Bom Dia, Verônica", "Raphael Montes", null, GeneroLivro.Suspense, 2016,
                "Uma investigadora da polícia civil cruza o caminho de um assassino em série enquanto tenta provar, sozinha, que o caso de uma vítima arquivado como suicídio é, na verdade, um crime.",
                StatusLeitura.Lendo, null, ProximoCarimbo()),

            // --- Leigh Bardugo (fantasia) ---
            NovoLivro("Sombra e Ossos", "Leigh Bardugo", "Trilogia Grisha", GeneroLivro.Fantasia, 2012,
                "Alina Starkov descobre ter um poder raro capaz de mudar o equilíbrio do reino, e é levada à corte para ser treinada — mas nem tudo ali é o que parece.",
                StatusLeitura.Lido, 4, ProximoCarimbo()),
            NovoLivro("Ruína e Ascensão", "Leigh Bardugo", "Trilogia Grisha", GeneroLivro.Fantasia, 2014,
                "No fechamento da trilogia Grisha, Alina precisa decidir até onde está disposta a ir para derrotar a sombra que ameaça destruir seu país.",
                StatusLeitura.QueroLer, null, ProximoCarimbo()),
            NovoLivro("Seis de Corvos", "Leigh Bardugo", "Seis de Corvos", GeneroLivro.Fantasia, 2015,
                "Um golpe daria a Kaz Brekker e sua gangue mais dinheiro do que eles jamais sonharam — se sobreviverem à missão quase impossível que aceitaram cumprir.",
                StatusLeitura.Lido, 5, ProximoCarimbo()),

            // --- Mariana Zapata (romance contemporâneo) ---
            NovoLivro("Under Locke", "Mariana Zapata", null, GeneroLivro.Romance, 2013,
                "Romance contemporâneo no estilo 'slow burn' característico da autora, sobre dois vizinhos que vão, aos poucos, abrindo espaço um para o outro.",
                StatusLeitura.QueroLer, null, ProximoCarimbo()),
            NovoLivro("The Wall of Winnipeg and Me", "Mariana Zapata", null, GeneroLivro.Romance, 2014,
                "A assistente pessoal de um jogador de futebol americano reservado precisa lidar com a proximidade diária com o chefe — e com sentimentos que não esperava sentir por ele.",
                StatusLeitura.Lido, 4, ProximoCarimbo()),
            NovoLivro("Kulti", "Mariana Zapata", null, GeneroLivro.Romance, 2015,
                "Uma ex-jogadora de futebol reencontra o ídolo da sua adolescência, agora como técnico, e precisa separar a admiração antiga dos sentimentos que surgem no presente.",
                StatusLeitura.Lendo, null, ProximoCarimbo()),
            NovoLivro("Wait for It", "Mariana Zapata", null, GeneroLivro.Romance, 2016,
                "Uma mulher que está sempre colocando a vida dos outros à frente da sua própria passa a conviver mais de perto com o primo do seu ex-cunhado, numa relação que cresce devagar.",
                StatusLeitura.QueroLer, null, ProximoCarimbo()),
            NovoLivro("From Lukov with Love", "Mariana Zapata", null, GeneroLivro.Romance, 2018,
                "Uma patinadora artística que nunca conquistou o reconhecimento que acha que merece encontra, num parceiro improvável, um apoio que não esperava.",
                StatusLeitura.QueroLer, null, ProximoCarimbo())
        };

        // Segunda passada: cada livro recebe um carimbo um pouco mais recente
        // que o anterior, então o ÚLTIMO da lista acima é o que aparece como
        // "último adicionado" no dashboard. Livros "Lido" também ganham
        // DataConclusao igual ao carimbo — é o que alimenta a meta de
        // leitura anual (ver LivroService.ObterDashboardAsync) já com
        // números de demonstração, em vez de ficar zerada.
        for (var i = 0; i < livros.Count; i++)
        {
            var carimbo = agora.AddMinutes(-(livros.Count - 1 - i));
            livros[i].CriadoEm = carimbo;
            livros[i].AtualizadoEm = carimbo;
            livros[i].DataConclusao = livros[i].StatusLeitura == StatusLeitura.Lido ? carimbo : null;
        }

        livros.AddRange(ObterEstanteAmigaLeitora(agora));

        return livros;
    }

    /// <summary>Estante pequena da segunda conta semeada (ver
    /// UsuarioAmigaLeitoraId) — alguns livros com resenha escrita, pra
    /// demonstrar o recurso de "seguir" já mostrando algo real quando a
    /// conta demo seguir esta conta.</summary>
    private static IReadOnlyList<Livro> ObterEstanteAmigaLeitora(DateTime agora)
    {
        var livros = new List<Livro>
        {
            NovoLivroPara(UsuarioAmigaLeitoraId, "Mulheres que Correm com os Lobos", "Clarissa Pinkola Estés", null,
                GeneroLivro.Outro, 1992,
                "Um ensaio sobre arquétipos femininos em contos populares e mitos de diferentes culturas, lido aos poucos.",
                StatusLeitura.Lendo, null, agora.AddDays(-3)),
            NovoLivroPara(UsuarioAmigaLeitoraId, "A Guerra dos Tronos", "George R. R. Martin", "As Crônicas de Gelo e Fogo",
                GeneroLivro.Fantasia, 1996,
                "Sete famílias disputam o Trono de Ferro enquanto um inverno que dura gerações se aproxima dos Sete Reinos.",
                StatusLeitura.Lido, 5, agora.AddDays(-20)),
            NovoLivroPara(UsuarioAmigaLeitoraId, "Admirável Mundo Novo", "Aldous Huxley", null,
                GeneroLivro.Outro, 1932,
                "Uma distopia sobre uma sociedade futura controlada pelo condicionamento genético e social desde o nascimento.",
                StatusLeitura.Lido, 4, agora.AddDays(-45)),
            NovoLivroPara(UsuarioAmigaLeitoraId, "O Nome do Vento", "Patrick Rothfuss", "Crônica do Matador do Rei",
                GeneroLivro.Fantasia, 2007,
                "Kvothe narra, em primeira pessoa, sua própria lenda — de criança numa trupe itinerante a um dos magos mais temidos do mundo.",
                StatusLeitura.QueroLer, null, agora.AddDays(-1))
        };

        livros[0].PaginaAtual = 140;
        livros[0].NumeroPaginas = 528;

        livros[1].Resenha = "Releitura. Continua sendo o melhor livro da série de longe — a política é implacável e ninguém está seguro.";
        livros[2].Resenha = "Incomoda de um jeito bom. Dá pra ver claramente de onde vieram várias distopias que li depois.";

        return livros;
    }

    private static Livro NovoLivro(
        string titulo, string autor, string? serie, GeneroLivro genero, int ano,
        string sinopse, StatusLeitura status, int? avaliacao, DateTime criadoEm) =>
        NovoLivroPara(UsuarioDemoId, titulo, autor, serie, genero, ano, sinopse, status, avaliacao, criadoEm);

    private static Livro NovoLivroPara(
        Guid usuarioId, string titulo, string autor, string? serie, GeneroLivro genero, int ano,
        string sinopse, StatusLeitura status, int? avaliacao, DateTime criadoEm) => new()
    {
        Id = Guid.NewGuid(),
        UsuarioId = usuarioId,
        Titulo = titulo,
        Autor = autor,
        Serie = serie,
        Genero = genero,
        AnoPublicacao = ano,
        SinopseCurta = sinopse,
        NumeroPaginas = null,
        StatusLeitura = status,
        Avaliacao = avaliacao,
        DataConclusao = status == StatusLeitura.Lido ? criadoEm : null,
        CriadoEm = criadoEm,
        AtualizadoEm = criadoEm
    };
}

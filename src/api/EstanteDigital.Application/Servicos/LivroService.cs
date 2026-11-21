using EstanteDigital.Application.DTOs;
using EstanteDigital.Application.Excecoes;
using EstanteDigital.Application.Interfaces;
using EstanteDigital.Domain.Entidades;
using EstanteDigital.Domain.Enums;
using EstanteDigital.Domain.Repositorios;

namespace EstanteDigital.Application.Servicos;

/// <summary>
/// Onde moram as regras de negócio de verdade. O controller não valida nada
/// sozinho — só recebe o DTO, chama o Service e devolve o resultado (ou o
/// erro). Isso é o que o PDF da Aula 20 chama de "Services com regras" no
/// checklist de code review.
/// </summary>
public class LivroService : ILivroService
{
    private readonly ILivroRepository _repositorio;
    private readonly IUsuarioRepository _usuarioRepository;

    public LivroService(ILivroRepository repositorio, IUsuarioRepository usuarioRepository)
    {
        _repositorio = repositorio;
        _usuarioRepository = usuarioRepository;
    }

    public async Task<IReadOnlyList<LivroDto>> ListarAsync(Guid usuarioId, string? genero, string? autor, string? status, string? busca)
    {
        var livros = await _repositorio.ListarTodosAsync(usuarioId);

        IEnumerable<Livro> consulta = livros;

        if (!string.IsNullOrWhiteSpace(genero) && Enum.TryParse<GeneroLivro>(genero, true, out var generoFiltro))
        {
            consulta = consulta.Where(l => l.Genero == generoFiltro);
        }

        if (!string.IsNullOrWhiteSpace(autor))
        {
            consulta = consulta.Where(l => l.Autor.Contains(autor, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<StatusLeitura>(status, true, out var statusFiltro))
        {
            consulta = consulta.Where(l => l.StatusLeitura == statusFiltro);
        }

        if (!string.IsNullOrWhiteSpace(busca))
        {
            consulta = consulta.Where(l =>
                l.Titulo.Contains(busca, StringComparison.OrdinalIgnoreCase) ||
                l.Autor.Contains(busca, StringComparison.OrdinalIgnoreCase) ||
                (l.Serie != null && l.Serie.Contains(busca, StringComparison.OrdinalIgnoreCase)));
        }

        return consulta
            .OrderByDescending(l => l.CriadoEm)
            .Select(ParaDto)
            .ToList();
    }

    public async Task<LivroDto?> BuscarPorIdAsync(Guid id, Guid usuarioId)
    {
        var livro = await _repositorio.BuscarPorIdAsync(id, usuarioId);
        return livro is null ? null : ParaDto(livro);
    }

    public async Task<LivroDto> CriarAsync(Guid usuarioId, CriarLivroDto dto)
    {
        var genero = ValidarGenero(dto.Genero);
        var status = ValidarStatus(dto.StatusLeitura ?? nameof(StatusLeitura.QueroLer));
        ValidarCamposComuns(dto.Titulo, dto.Autor, dto.AnoPublicacao, dto.SinopseCurta);
        ValidarAvaliacao(dto.Avaliacao, status);
        ValidarPaginaAtual(dto.PaginaAtual, dto.NumeroPaginas);
        ValidarResenha(dto.Resenha);

        var livro = new Livro
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            Titulo = dto.Titulo.Trim(),
            Autor = dto.Autor.Trim(),
            Serie = string.IsNullOrWhiteSpace(dto.Serie) ? null : dto.Serie.Trim(),
            Genero = genero,
            AnoPublicacao = dto.AnoPublicacao,
            SinopseCurta = dto.SinopseCurta.Trim(),
            NumeroPaginas = dto.NumeroPaginas,
            StatusLeitura = status,
            Avaliacao = dto.Avaliacao,
            PaginaAtual = dto.PaginaAtual,
            Resenha = string.IsNullOrWhiteSpace(dto.Resenha) ? null : dto.Resenha.Trim(),
            // Livro já cadastrado direto como "Lido" conta como concluído agora
            // (não há como a pessoa informar uma data de conclusão no passado
            // nesta versão — ver LEIA-ME, decisão tomada sem perguntar).
            DataConclusao = status == StatusLeitura.Lido ? DateTime.UtcNow : null,
            CriadoEm = DateTime.UtcNow,
            AtualizadoEm = DateTime.UtcNow
        };

        var criado = await _repositorio.AdicionarAsync(livro);
        return ParaDto(criado);
    }

    public async Task<LivroDto?> AtualizarAsync(Guid id, Guid usuarioId, AtualizarLivroDto dto)
    {
        var existente = await _repositorio.BuscarPorIdAsync(id, usuarioId);
        if (existente is null)
        {
            return null;
        }

        var genero = ValidarGenero(dto.Genero);
        var status = ValidarStatus(dto.StatusLeitura);
        ValidarCamposComuns(dto.Titulo, dto.Autor, dto.AnoPublicacao, dto.SinopseCurta);
        ValidarAvaliacao(dto.Avaliacao, status);
        ValidarPaginaAtual(dto.PaginaAtual, dto.NumeroPaginas);
        ValidarResenha(dto.Resenha);

        var statusAnterior = existente.StatusLeitura;

        existente.Titulo = dto.Titulo.Trim();
        existente.Autor = dto.Autor.Trim();
        existente.Serie = string.IsNullOrWhiteSpace(dto.Serie) ? null : dto.Serie.Trim();
        existente.Genero = genero;
        existente.AnoPublicacao = dto.AnoPublicacao;
        existente.SinopseCurta = dto.SinopseCurta.Trim();
        existente.NumeroPaginas = dto.NumeroPaginas;
        existente.StatusLeitura = status;
        existente.Avaliacao = dto.Avaliacao;
        existente.PaginaAtual = dto.PaginaAtual;
        existente.Resenha = string.IsNullOrWhiteSpace(dto.Resenha) ? null : dto.Resenha.Trim();
        existente.AtualizadoEm = DateTime.UtcNow;

        // DataConclusao é derivada da mudança de status, não um campo que o
        // formulário envia direto: só "nasce" quando o status vira Lido pela
        // primeira vez (preservando a data original se a pessoa só editar
        // outros campos depois, sem mexer no status) e some assim que o
        // status deixa de ser Lido.
        if (status == StatusLeitura.Lido)
        {
            if (statusAnterior != StatusLeitura.Lido || existente.DataConclusao is null)
            {
                existente.DataConclusao = DateTime.UtcNow;
            }
        }
        else
        {
            existente.DataConclusao = null;
        }

        var atualizado = await _repositorio.AtualizarAsync(existente);
        return atualizado is null ? null : ParaDto(atualizado);
    }

    public Task<bool> RemoverAsync(Guid id, Guid usuarioId) => _repositorio.RemoverAsync(id, usuarioId);

    public async Task<DashboardDto> ObterDashboardAsync(Guid usuarioId)
    {
        var livros = await _repositorio.ListarTodosAsync(usuarioId);
        var usuario = await _usuarioRepository.BuscarPorIdAsync(usuarioId);

        var anoAtual = DateTime.UtcNow.Year;
        var livrosLidosNoAnoAtual = livros.Count(l => l.DataConclusao.HasValue && l.DataConclusao.Value.Year == anoAtual);

        var lidos = livros.Where(l => l.StatusLeitura == StatusLeitura.Lido).ToList();
        var avaliacoesValidas = lidos.Where(l => l.Avaliacao.HasValue).Select(l => l.Avaliacao!.Value).ToList();

        var porGenero = livros
            .GroupBy(l => l.Genero.ToString())
            .Select(g => new ContagemPorChaveDto(g.Key, g.Count()))
            .OrderByDescending(c => c.Quantidade)
            .ToList();

        var porAutor = livros
            .GroupBy(l => l.Autor)
            .Select(g => new ContagemPorChaveDto(g.Key, g.Count()))
            .OrderByDescending(c => c.Quantidade)
            .ToList();

        var ultimoAdicionado = livros
            .OrderByDescending(l => l.CriadoEm)
            .Select(ParaDto)
            .FirstOrDefault();

        return new DashboardDto(
            TotalDeLivros: livros.Count,
            TotalLidos: lidos.Count,
            TotalLendo: livros.Count(l => l.StatusLeitura == StatusLeitura.Lendo),
            TotalQueroLer: livros.Count(l => l.StatusLeitura == StatusLeitura.QueroLer),
            MediaAvaliacaoLidos: avaliacoesValidas.Count > 0 ? Math.Round(avaliacoesValidas.Average(), 2) : null,
            PorGenero: porGenero,
            PorAutor: porAutor,
            UltimoAdicionado: ultimoAdicionado,
            MetaLeituraAnual: usuario?.MetaLeituraAnual,
            LivrosLidosNoAnoAtual: livrosLidosNoAnoAtual
        );
    }

    /// <summary>
    /// Ranking dos livros melhor avaliados (estilo "melhores avaliados" do
    /// Skoob). Só entram livros com StatusLeitura == Lido e Avaliacao
    /// preenchida — um livro "Quero ler" não tem nota, não faz sentido
    /// competir num ranking por nota. Em empate, o critério de desempate é
    /// o mais recente adicionado primeiro (mesma lógica usada no dashboard
    /// para "último adicionado").
    /// </summary>
    public async Task<IReadOnlyList<LivroDto>> ObterRankingAsync(Guid usuarioId)
    {
        var livros = await _repositorio.ListarTodosAsync(usuarioId);

        return livros
            .Where(l => l.StatusLeitura == StatusLeitura.Lido && l.Avaliacao.HasValue)
            .OrderByDescending(l => l.Avaliacao)
            .ThenByDescending(l => l.CriadoEm)
            .Select(ParaDto)
            .ToList();
    }

    private static GeneroLivro ValidarGenero(string genero)
    {
        if (!Enum.TryParse<GeneroLivro>(genero, true, out var valor))
        {
            throw new ValidacaoException(
                $"Gênero inválido: '{genero}'. Valores aceitos: {string.Join(", ", Enum.GetNames<GeneroLivro>())}.");
        }

        return valor;
    }

    private static StatusLeitura ValidarStatus(string status)
    {
        if (!Enum.TryParse<StatusLeitura>(status, true, out var valor))
        {
            throw new ValidacaoException(
                $"Status de leitura inválido: '{status}'. Valores aceitos: {string.Join(", ", Enum.GetNames<StatusLeitura>())}.");
        }

        return valor;
    }

    private static void ValidarCamposComuns(string titulo, string autor, int anoPublicacao, string sinopse)
    {
        if (string.IsNullOrWhiteSpace(titulo) || titulo.Trim().Length < 2)
        {
            throw new ValidacaoException("O título deve ter pelo menos 2 caracteres.");
        }

        if (string.IsNullOrWhiteSpace(autor) || autor.Trim().Length < 2)
        {
            throw new ValidacaoException("O autor deve ter pelo menos 2 caracteres.");
        }

        var anoAtual = DateTime.UtcNow.Year;
        if (anoPublicacao < 1450 || anoPublicacao > anoAtual + 1)
        {
            throw new ValidacaoException($"Ano de publicação inválido. Use um valor entre 1450 e {anoAtual + 1}.");
        }

        if (string.IsNullOrWhiteSpace(sinopse) || sinopse.Trim().Length < 10)
        {
            throw new ValidacaoException("A sinopse deve ter pelo menos 10 caracteres.");
        }
    }

    private static void ValidarAvaliacao(int? avaliacao, StatusLeitura status)
    {
        if (avaliacao is null)
        {
            return;
        }

        if (avaliacao < 1 || avaliacao > 5)
        {
            throw new ValidacaoException("A avaliação deve ser um número entre 1 e 5.");
        }

        if (status != StatusLeitura.Lido)
        {
            throw new ValidacaoException("Só é possível avaliar um livro que já foi marcado como 'Lido'.");
        }
    }

    /// <summary>Não exige StatusLeitura == Lendo de propósito: a pessoa pode
    /// querer manter a página registrada mesmo depois de marcar como Lido
    /// (ex.: "terminei na página 320 de um livro de 320"), e travar isso por
    /// status só criaria fricção sem ganho real.</summary>
    private static void ValidarPaginaAtual(int? paginaAtual, int? numeroPaginas)
    {
        if (paginaAtual is null)
        {
            return;
        }

        if (paginaAtual < 0)
        {
            throw new ValidacaoException("A página atual não pode ser negativa.");
        }

        if (numeroPaginas.HasValue && paginaAtual > numeroPaginas)
        {
            throw new ValidacaoException("A página atual não pode ser maior que o número total de páginas.");
        }
    }

    private static void ValidarResenha(string? resenha)
    {
        if (!string.IsNullOrWhiteSpace(resenha) && resenha.Trim().Length > 4000)
        {
            throw new ValidacaoException("A resenha deve ter no máximo 4000 caracteres.");
        }
    }

    private static LivroDto ParaDto(Livro livro) => new(
        livro.Id,
        livro.Titulo,
        livro.Autor,
        livro.Serie,
        livro.Genero.ToString(),
        livro.AnoPublicacao,
        livro.SinopseCurta,
        livro.NumeroPaginas,
        livro.StatusLeitura.ToString(),
        livro.Avaliacao,
        livro.PaginaAtual,
        livro.Resenha,
        livro.DataConclusao,
        livro.CriadoEm,
        livro.AtualizadoEm
    );
}

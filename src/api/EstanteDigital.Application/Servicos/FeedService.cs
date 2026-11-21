using EstanteDigital.Application.DTOs;
using EstanteDigital.Application.Interfaces;
using EstanteDigital.Domain.Entidades;
using EstanteDigital.Domain.Enums;
using EstanteDigital.Domain.Repositorios;

namespace EstanteDigital.Application.Servicos;

/// <summary>
/// Monta o feed de atividade de quem o usuário segue, misturando dois tipos
/// de fonte: eventos persistidos (Atividade — ver LivroService/StreakService,
/// que são quem realmente grava essas linhas no momento em que acontecem) e
/// o único evento calculado na hora, "desafio concluído" (ver TipoAtividade
/// para o porquê de não ser persistido). Os dois viram o mesmo formato de
/// saída (AtividadeDto), então pro front é tudo igual.
/// </summary>
public class FeedService : IFeedService
{
    private const int LimiteAtividades = 50;

    private readonly ISeguidorRepository _seguidorRepository;
    private readonly IAtividadeRepository _atividadeRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ILivroRepository _livroRepository;
    private readonly IDesafioRepository _desafioRepository;
    private readonly IParticipacaoDesafioRepository _participacaoRepository;

    public FeedService(
        ISeguidorRepository seguidorRepository,
        IAtividadeRepository atividadeRepository,
        IUsuarioRepository usuarioRepository,
        ILivroRepository livroRepository,
        IDesafioRepository desafioRepository,
        IParticipacaoDesafioRepository participacaoRepository)
    {
        _seguidorRepository = seguidorRepository;
        _atividadeRepository = atividadeRepository;
        _usuarioRepository = usuarioRepository;
        _livroRepository = livroRepository;
        _desafioRepository = desafioRepository;
        _participacaoRepository = participacaoRepository;
    }

    public async Task<IReadOnlyList<AtividadeDto>> ObterFeedAsync(Guid usuarioId)
    {
        var seguindoIds = await _seguidorRepository.ListarSeguindoIdsAsync(usuarioId);
        if (seguindoIds.Count == 0)
        {
            return Array.Empty<AtividadeDto>();
        }

        // Cache simples só pra não buscar o mesmo usuário várias vezes
        // dentro desta mesma chamada (um usuário pode aparecer em várias
        // atividades diferentes do feed).
        var cacheUsuarios = new Dictionary<Guid, Usuario?>();
        async Task<Usuario?> ObterUsuarioAsync(Guid id)
        {
            if (!cacheUsuarios.TryGetValue(id, out var usuario))
            {
                usuario = await _usuarioRepository.BuscarPorIdAsync(id);
                cacheUsuarios[id] = usuario;
            }
            return usuario;
        }

        var resultado = new List<AtividadeDto>();

        // --- Eventos persistidos: terminou livro, resenha nova, meta batida, check-in ---
        var atividades = await _atividadeRepository.ListarDeUsuariosAsync(seguindoIds, LimiteAtividades);
        foreach (var atividade in atividades)
        {
            var usuario = await ObterUsuarioAsync(atividade.UsuarioId);
            if (usuario is null)
            {
                continue;
            }

            string? livroTitulo = null;
            if (atividade.LivroId.HasValue)
            {
                var livro = await _livroRepository.BuscarPorIdAsync(atividade.LivroId.Value, atividade.UsuarioId);
                livroTitulo = livro?.Titulo;
            }

            resultado.Add(new AtividadeDto(
                atividade.Id,
                usuario.Id,
                usuario.NomeUsuario,
                usuario.Icone,
                usuario.CorPrincipal,
                atividade.Tipo.ToString(),
                atividade.CriadoEm,
                atividade.LivroId,
                livroTitulo,
                atividade.MetaValor,
                atividade.SequenciaDias,
                null,
                null,
                null
            ));
        }

        // --- Evento calculado: desafios encerrados em que algum seguido participou ---
        foreach (var seguidoId in seguindoIds)
        {
            var usuario = await ObterUsuarioAsync(seguidoId);
            if (usuario is null)
            {
                continue;
            }

            var desafioIds = await _participacaoRepository.ListarDesafioIdsDoUsuarioAsync(seguidoId);
            foreach (var desafioId in desafioIds)
            {
                var desafio = await _desafioRepository.BuscarPorIdAsync(desafioId);
                if (desafio is null || DateTime.UtcNow.Date <= desafio.DataFim.Date)
                {
                    continue; // ainda não encerrou
                }

                var livros = await _livroRepository.ListarTodosAsync(seguidoId);
                var lidosNoPeriodo = livros.Count(l =>
                    l.DataConclusao.HasValue &&
                    l.DataConclusao.Value.Date >= desafio.DataInicio.Date &&
                    l.DataConclusao.Value.Date <= desafio.DataFim.Date);

                resultado.Add(new AtividadeDto(
                    CombinarGuids(seguidoId, desafioId),
                    usuario.Id,
                    usuario.NomeUsuario,
                    usuario.Icone,
                    usuario.CorPrincipal,
                    TipoAtividade.DesafioConcluido.ToString(),
                    desafio.DataFim,
                    null,
                    null,
                    null,
                    null,
                    desafio.Id,
                    desafio.Titulo,
                    lidosNoPeriodo
                ));
            }
        }

        return resultado
            .OrderByDescending(a => a.CriadoEm)
            .Take(LimiteAtividades)
            .ToList();
    }

    /// <summary>Combina dois Guids num terceiro, determinístico (XOR byte a
    /// byte) — usado só pra dar um Id estável à linha "desafio concluído"
    /// do feed, que não existe como registro de verdade em lugar nenhum
    /// (ver comentário em TipoAtividade.DesafioConcluido).</summary>
    private static Guid CombinarGuids(Guid a, Guid b)
    {
        var bytesA = a.ToByteArray();
        var bytesB = b.ToByteArray();
        var resultado = new byte[16];
        for (var i = 0; i < 16; i++)
        {
            resultado[i] = (byte)(bytesA[i] ^ bytesB[i]);
        }

        return new Guid(resultado);
    }
}

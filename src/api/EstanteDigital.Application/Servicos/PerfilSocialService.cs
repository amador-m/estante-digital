using EstanteDigital.Application.DTOs;
using EstanteDigital.Application.Excecoes;
using EstanteDigital.Application.Interfaces;
using EstanteDigital.Domain.Entidades;
using EstanteDigital.Domain.Repositorios;

namespace EstanteDigital.Application.Servicos;

/// <summary>
/// Diferente do PerfilService (que fica em Infrastructure por depender do
/// SenhaHasher, um detalhe de infraestrutura), este serviço só depende de
/// interfaces de repositório do Domain — então fica aqui em Application,
/// igual o LivroService. Não é inconsistência com o padrão anterior: é o
/// mesmo critério ("depende de um detalhe de infra concreto? Infrastructure.
/// Só de abstrações do Domain? Application") aplicado de novo, só que desta
/// vez o resultado é diferente porque a dependência é diferente.
/// </summary>
public class PerfilSocialService : IPerfilSocialService
{
    private const int LimiteResultadosBusca = 20;

    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ISeguidorRepository _seguidorRepository;
    private readonly ILivroRepository _livroRepository;

    public PerfilSocialService(
        IUsuarioRepository usuarioRepository,
        ISeguidorRepository seguidorRepository,
        ILivroRepository livroRepository)
    {
        _usuarioRepository = usuarioRepository;
        _seguidorRepository = seguidorRepository;
        _livroRepository = livroRepository;
    }

    public async Task<IReadOnlyList<UsuarioPublicoDto>> BuscarUsuariosAsync(Guid solicitanteId, string trecho)
    {
        if (string.IsNullOrWhiteSpace(trecho) || trecho.Trim().Length < 2)
        {
            throw new ValidacaoException("Digite pelo menos 2 caracteres para buscar.");
        }

        var encontrados = await _usuarioRepository.BuscarPorNomeParcialAsync(trecho.Trim());

        return encontrados
            .Where(u => u.Id != solicitanteId)
            .Take(LimiteResultadosBusca)
            .Select(ParaUsuarioPublicoDto)
            .ToList();
    }

    public async Task SeguirAsync(Guid solicitanteId, string nomeUsuarioAlvo)
    {
        var alvo = await BuscarAlvoOuFalharAsync(nomeUsuarioAlvo);

        if (alvo.Id == solicitanteId)
        {
            throw new ValidacaoException("Você não pode seguir sua própria conta.");
        }

        await _seguidorRepository.SeguirAsync(solicitanteId, alvo.Id);
    }

    public async Task DeixarDeSeguirAsync(Guid solicitanteId, string nomeUsuarioAlvo)
    {
        var alvo = await BuscarAlvoOuFalharAsync(nomeUsuarioAlvo);
        await _seguidorRepository.DeixarDeSeguirAsync(solicitanteId, alvo.Id);
    }

    public async Task<PerfilPublicoDto> ObterPerfilPublicoAsync(Guid solicitanteId, string nomeUsuarioAlvo)
    {
        var alvo = await BuscarAlvoOuFalharAsync(nomeUsuarioAlvo);

        var ehVoceMesmo = alvo.Id == solicitanteId;
        var voceSegue = !ehVoceMesmo && await _seguidorRepository.EstaSeguindoAsync(solicitanteId, alvo.Id);

        var totalSeguidores = await _seguidorRepository.ContarSeguidoresAsync(alvo.Id);
        var totalSeguindo = await _seguidorRepository.ContarSeguindoAsync(alvo.Id);

        var livrosDoAlvo = await _livroRepository.ListarTodosAsync(alvo.Id);
        var anoAtual = DateTime.UtcNow.Year;
        var livrosLidosNoAnoAtual = livrosDoAlvo.Count(l => l.DataConclusao.HasValue && l.DataConclusao.Value.Year == anoAtual);

        // A estante só é exposta pra quem segue (ou pro próprio dono). Pra
        // quem só está "espiando" antes de decidir seguir, dá pra ver nome/ícone/cor e as
        // estatísticas (quantos livros, meta, seguidores), mas não os
        // livros em si nem as resenhas.
        var estanteVisivel = (ehVoceMesmo || voceSegue)
            ? livrosDoAlvo.OrderByDescending(l => l.CriadoEm).Select(ParaLivroDto).ToList()
            : new List<LivroDto>();

        return new PerfilPublicoDto(
            alvo.Id,
            alvo.NomeUsuario,
            alvo.Icone,
            alvo.CorPrincipal,
            alvo.CriadoEm,
            alvo.MetaLeituraAnual,
            livrosLidosNoAnoAtual,
            totalSeguidores,
            totalSeguindo,
            voceSegue,
            ehVoceMesmo,
            estanteVisivel
        );
    }

    public async Task<IReadOnlyList<UsuarioPublicoDto>> ListarSeguindoAsync(Guid usuarioId)
    {
        var ids = await _seguidorRepository.ListarSeguindoIdsAsync(usuarioId);
        return await ResolverUsuariosAsync(ids);
    }

    public async Task<IReadOnlyList<UsuarioPublicoDto>> ListarSeguidoresAsync(Guid usuarioId)
    {
        var ids = await _seguidorRepository.ListarSeguidoresIdsAsync(usuarioId);
        return await ResolverUsuariosAsync(ids);
    }

    private async Task<Usuario> BuscarAlvoOuFalharAsync(string nomeUsuarioAlvo)
    {
        var alvo = await _usuarioRepository.BuscarPorNomeUsuarioAsync(nomeUsuarioAlvo.Trim());
        return alvo ?? throw new ValidacaoException("Usuário não encontrado.");
    }

    private async Task<IReadOnlyList<UsuarioPublicoDto>> ResolverUsuariosAsync(IReadOnlyList<Guid> ids)
    {
        var resultado = new List<UsuarioPublicoDto>();
        foreach (var id in ids)
        {
            var usuario = await _usuarioRepository.BuscarPorIdAsync(id);
            if (usuario is not null)
            {
                resultado.Add(ParaUsuarioPublicoDto(usuario));
            }
        }

        return resultado;
    }

    private static UsuarioPublicoDto ParaUsuarioPublicoDto(Usuario usuario) => new(
        usuario.Id,
        usuario.NomeUsuario,
        usuario.Icone,
        usuario.CorPrincipal
    );

    // Mesma forma de converter Livro -> LivroDto que o LivroService usa.
    // Duplicar essas poucas linhas (em vez de expor um método público
    // estático em LivroService só pra isso) manteve os dois serviços
    // independentes um do outro — nenhum precisa saber que o outro existe.
    private static LivroDto ParaLivroDto(Livro livro) => new(
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

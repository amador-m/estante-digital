using EstanteDigital.Application.DTOs;
using EstanteDigital.Application.Excecoes;
using EstanteDigital.Application.Interfaces;
using EstanteDigital.Domain.Entidades;
using EstanteDigital.Domain.Repositorios;

namespace EstanteDigital.Application.Servicos;

/// <summary>
/// Desafios de leitura em grupo (Fase 4, estilo GymRats). Participação é
/// aberta — qualquer conta pode criar e qualquer conta pode entrar em
/// qualquer desafio, sem convite/aceite (decisão da tutoria: mais simples
/// de implementar e testar sozinha). O placar nunca é guardado — é
/// recalculado toda vez, contando Livro.DataConclusao de cada participante
/// dentro da janela do desafio (mesmo campo que já alimenta a meta de
/// leitura anual desde a Fase 3).
/// </summary>
public class DesafioService : IDesafioService
{
    private readonly IDesafioRepository _desafioRepository;
    private readonly IParticipacaoDesafioRepository _participacaoRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ILivroRepository _livroRepository;

    public DesafioService(
        IDesafioRepository desafioRepository,
        IParticipacaoDesafioRepository participacaoRepository,
        IUsuarioRepository usuarioRepository,
        ILivroRepository livroRepository)
    {
        _desafioRepository = desafioRepository;
        _participacaoRepository = participacaoRepository;
        _usuarioRepository = usuarioRepository;
        _livroRepository = livroRepository;
    }

    public async Task<DesafioDto> CriarAsync(Guid criadorId, CriarDesafioDto dto)
    {
        ValidarDatas(dto.DataInicio, dto.DataFim);

        if (string.IsNullOrWhiteSpace(dto.Titulo) || dto.Titulo.Trim().Length < 3)
        {
            throw new ValidacaoException("O título do desafio deve ter pelo menos 3 caracteres.");
        }

        if (!string.IsNullOrWhiteSpace(dto.Descricao) && dto.Descricao.Trim().Length > 1000)
        {
            throw new ValidacaoException("A descrição deve ter no máximo 1000 caracteres.");
        }

        var desafio = new Desafio
        {
            Id = Guid.NewGuid(),
            CriadorId = criadorId,
            Titulo = dto.Titulo.Trim(),
            Descricao = string.IsNullOrWhiteSpace(dto.Descricao) ? null : dto.Descricao.Trim(),
            DataInicio = dto.DataInicio.Date,
            DataFim = dto.DataFim.Date,
            CriadoEm = DateTime.UtcNow
        };

        var criado = await _desafioRepository.CriarAsync(desafio);

        // Quem cria já entra automaticamente — senão o próprio desafio não
        // apareceria em "meus desafios" nem no placar de quem o criou, até
        // a pessoa clicar "entrar" no próprio desafio (fricção sem motivo).
        await _participacaoRepository.EntrarAsync(criado.Id, criadorId);

        return await ParaDesafioDtoAsync(criado, criadorId);
    }

    public async Task<IReadOnlyList<DesafioDto>> ListarTodosAsync(Guid usuarioId)
    {
        var desafios = await _desafioRepository.ListarTodosAsync();
        var dtos = new List<DesafioDto>();
        foreach (var desafio in desafios)
        {
            dtos.Add(await ParaDesafioDtoAsync(desafio, usuarioId));
        }

        // Ativos primeiro (os que terminam mais cedo no topo, criando senso
        // de urgência), encerrados depois (mais recentes primeiro).
        return dtos
            .OrderBy(d => d.Status == "Encerrado" ? 1 : 0)
            .ThenBy(d => d.Status == "Ativo" ? d.DataFim : DateTime.MaxValue)
            .ThenByDescending(d => d.Status == "Encerrado" ? d.DataFim : DateTime.MinValue)
            .ToList();
    }

    public async Task<IReadOnlyList<DesafioDto>> ListarMeusAsync(Guid usuarioId)
    {
        var todos = await ListarTodosAsync(usuarioId);
        return todos.Where(d => d.VoceParticipa).ToList();
    }

    public async Task<DesafioDetalheDto?> ObterDetalheAsync(Guid id, Guid usuarioId)
    {
        var desafio = await _desafioRepository.BuscarPorIdAsync(id);
        if (desafio is null)
        {
            return null;
        }

        var desafioDto = await ParaDesafioDtoAsync(desafio, usuarioId);

        var participantesIds = await _participacaoRepository.ListarParticipantesIdsAsync(id);
        var placar = new List<ParticipanteDesafioDto>();
        foreach (var participanteId in participantesIds)
        {
            var usuario = await _usuarioRepository.BuscarPorIdAsync(participanteId);
            if (usuario is null)
            {
                continue;
            }

            var livros = await _livroRepository.ListarTodosAsync(participanteId);
            var lidosNoPeriodo = livros.Count(l =>
                l.DataConclusao.HasValue &&
                l.DataConclusao.Value.Date >= desafio.DataInicio.Date &&
                l.DataConclusao.Value.Date <= desafio.DataFim.Date);

            placar.Add(new ParticipanteDesafioDto(usuario.Id, usuario.NomeUsuario, usuario.Icone, usuario.CorPrincipal, lidosNoPeriodo));
        }

        var placarOrdenado = placar
            .OrderByDescending(p => p.LivrosLidosNoPeriodo)
            .ThenBy(p => p.NomeUsuario, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new DesafioDetalheDto(desafioDto, placarOrdenado);
    }

    public async Task EntrarAsync(Guid id, Guid usuarioId)
    {
        var desafio = await _desafioRepository.BuscarPorIdAsync(id) ?? throw new ValidacaoException("Desafio não encontrado.");

        if (CalcularStatus(desafio) == "Encerrado")
        {
            throw new ValidacaoException("Este desafio já foi encerrado — não é mais possível entrar.");
        }

        await _participacaoRepository.EntrarAsync(id, usuarioId);
    }

    public Task SairAsync(Guid id, Guid usuarioId) => _participacaoRepository.SairAsync(id, usuarioId);

    public async Task<bool> RemoverAsync(Guid id, Guid usuarioId)
    {
        var desafio = await _desafioRepository.BuscarPorIdAsync(id);
        if (desafio is null)
        {
            return false;
        }

        if (desafio.CriadorId != usuarioId)
        {
            throw new ValidacaoException("Só quem criou o desafio pode excluí-lo.");
        }

        await _participacaoRepository.RemoverTodasDoDesafioAsync(id);
        return await _desafioRepository.RemoverAsync(id, usuarioId);
    }

    private static void ValidarDatas(DateTime dataInicio, DateTime dataFim)
    {
        if (dataFim.Date <= dataInicio.Date)
        {
            throw new ValidacaoException("A data de fim deve ser depois da data de início.");
        }

        if (dataInicio.Date < DateTime.UtcNow.Date)
        {
            throw new ValidacaoException("A data de início não pode ser no passado.");
        }
    }

    private static string CalcularStatus(Desafio desafio) =>
        DateTime.UtcNow.Date > desafio.DataFim.Date ? "Encerrado" : "Ativo";

    private async Task<DesafioDto> ParaDesafioDtoAsync(Desafio desafio, Guid usuarioId)
    {
        var criador = await _usuarioRepository.BuscarPorIdAsync(desafio.CriadorId);
        var totalParticipantes = (await _participacaoRepository.ListarParticipantesIdsAsync(desafio.Id)).Count;
        var voceParticipa = await _participacaoRepository.ParticipaAsync(desafio.Id, usuarioId);

        return new DesafioDto(
            desafio.Id,
            desafio.Titulo,
            desafio.Descricao,
            desafio.DataInicio,
            desafio.DataFim,
            desafio.CriadorId,
            criador?.NomeUsuario ?? "(conta removida)",
            desafio.CriadoEm,
            totalParticipantes,
            voceParticipa,
            desafio.CriadorId == usuarioId,
            CalcularStatus(desafio)
        );
    }
}

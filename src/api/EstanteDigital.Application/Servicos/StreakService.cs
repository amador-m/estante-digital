using EstanteDigital.Application.DTOs;
using EstanteDigital.Application.Interfaces;
using EstanteDigital.Domain.Entidades;
using EstanteDigital.Domain.Enums;
using EstanteDigital.Domain.Repositorios;

namespace EstanteDigital.Application.Servicos;

/// <summary>
/// Streak de leitura (sequência de dias seguidos com check-in). A regra de
/// contagem segue o mesmo modelo de apps como Duolingo: a sequência conta
/// os dias consecutivos terminando hoje OU ontem (se a pessoa ainda não
/// marcou hoje, a sequência continua "viva" até o fim do dia — só quebra
/// de verdade quando um dia inteiro passa em branco).
/// </summary>
public class StreakService : IStreakService
{
    private readonly ICheckInRepository _checkInRepository;
    private readonly IAtividadeRepository _atividadeRepository;

    public StreakService(ICheckInRepository checkInRepository, IAtividadeRepository atividadeRepository)
    {
        _checkInRepository = checkInRepository;
        _atividadeRepository = atividadeRepository;
    }

    public async Task<StreakDto> ObterStatusAsync(Guid usuarioId)
    {
        var datas = await _checkInRepository.ListarDatasAsync(usuarioId);
        var diasComCheckIn = datas.Select(d => d.Date).ToHashSet();

        var hoje = DateTime.UtcNow.Date;
        var jaMarcouHoje = diasComCheckIn.Contains(hoje);

        var cursor = jaMarcouHoje ? hoje : hoje.AddDays(-1);
        var sequencia = 0;
        while (diasComCheckIn.Contains(cursor))
        {
            sequencia++;
            cursor = cursor.AddDays(-1);
        }

        return new StreakDto(sequencia, jaMarcouHoje);
    }

    public async Task<StreakDto> RegistrarCheckInAsync(Guid usuarioId)
    {
        var hoje = DateTime.UtcNow.Date;
        var jaExistia = await _checkInRepository.JaRegistrouAsync(usuarioId, hoje);

        await _checkInRepository.RegistrarAsync(usuarioId, hoje);
        var status = await ObterStatusAsync(usuarioId);

        // Só loga no feed na primeira vez que o dia é marcado — clicar de
        // novo no mesmo dia não gera um segundo evento (ver JaRegistrouAsync
        // acima, checado ANTES de registrar).
        if (!jaExistia)
        {
            await _atividadeRepository.RegistrarAsync(new Atividade
            {
                Id = Guid.NewGuid(),
                UsuarioId = usuarioId,
                Tipo = TipoAtividade.CheckIn,
                SequenciaDias = status.SequenciaAtual,
                CriadoEm = DateTime.UtcNow
            });
        }

        return status;
    }
}

namespace EstanteDigital.Application.DTOs;

/// <summary>Um desafio na listagem (sem o placar completo — ver
/// DesafioDetalheDto para isso, que só é montado quando alguém abre o
/// desafio específico, pra não calcular o placar de todo mundo numa lista).</summary>
public record DesafioDto(
    Guid Id,
    string Titulo,
    string? Descricao,
    DateTime DataInicio,
    DateTime DataFim,
    Guid CriadorId,
    string CriadorNomeUsuario,
    DateTime CriadoEm,
    int TotalParticipantes,
    bool VoceParticipa,
    bool VoceCriou,
    /// <summary>"Ativo" (hoje está dentro de [DataInicio, DataFim]) ou
    /// "Encerrado" (DataFim já passou) — calculado, nunca guardado, porque
    /// não existe nenhum processo em segundo plano neste projeto que
    /// pudesse manter um campo de status atualizado sozinho.</summary>
    string Status
);

public record CriarDesafioDto(string Titulo, string? Descricao, DateTime DataInicio, DateTime DataFim);

public record ParticipanteDesafioDto(
    Guid UsuarioId,
    string NomeUsuario,
    string Icone,
    string CorPrincipal,
    int LivrosLidosNoPeriodo
);

public record DesafioDetalheDto(DesafioDto Desafio, IReadOnlyList<ParticipanteDesafioDto> Placar);

namespace EstanteDigital.Domain.Entidades;

/// <summary>
/// Um desafio de leitura em grupo (estilo GymRats: prazo definido, placar
/// entre participantes). Participação é aberta — qualquer conta pode criar
/// e qualquer conta pode entrar em qualquer desafio existente, sem convite
/// nem aceite (mesma simplicidade de propósito que já valia para "seguir"
/// na Fase 3). O placar em si não é guardado aqui: é calculado sob demanda
/// (ver DesafioService), contando quantos livros cada participante marcou
/// como Lido dentro da janela [DataInicio, DataFim] — reaproveita
/// Livro.DataConclusao, o mesmo campo que já alimenta a meta de leitura
/// anual.
/// </summary>
public class Desafio
{
    public Guid Id { get; set; }

    /// <summary>Quem criou o desafio — único com permissão de excluí-lo.</summary>
    public Guid CriadorId { get; set; }

    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }

    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}

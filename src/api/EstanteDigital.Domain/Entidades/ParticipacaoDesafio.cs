namespace EstanteDigital.Domain.Entidades;

/// <summary>
/// "UsuarioId entrou no DesafioId" — relação simples de participação, sem
/// papel especial pro criador (quem cria também precisa "entrar" se quiser
/// aparecer no próprio placar; ver DesafioService).
/// </summary>
public class ParticipacaoDesafio
{
    public Guid Id { get; set; }
    public Guid DesafioId { get; set; }
    public Guid UsuarioId { get; set; }
    public DateTime EntrouEm { get; set; } = DateTime.UtcNow;
}

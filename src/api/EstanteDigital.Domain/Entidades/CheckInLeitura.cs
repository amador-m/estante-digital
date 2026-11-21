namespace EstanteDigital.Domain.Entidades;

/// <summary>
/// Um "marquei que li hoje" — a unidade do streak de leitura (sequência de
/// dias). Check-in é manual (a pessoa clica um botão), não inferido
/// automaticamente de outras ações no sistema: é o mesmo modelo do GymRats
/// registrar o treino do dia, e evita contar como "dia de leitura" um dia
/// em que a pessoa só editou um dado antigo sem ter lido nada.
/// </summary>
public class CheckInLeitura
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }

    /// <summary>Só a data importa (hora sempre truncada para 00:00 UTC) —
    /// é o "dia" que está sendo registrado, não o instante exato do clique.</summary>
    public DateTime Data { get; set; }

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}

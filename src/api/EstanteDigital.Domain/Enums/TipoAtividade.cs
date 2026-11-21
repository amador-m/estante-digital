namespace EstanteDigital.Domain.Enums;

/// <summary>
/// Os tipos de evento que aparecem no feed de atividade (Fase 4). Os quatro
/// primeiros correspondem a uma linha persistida em Atividade, registrada
/// no exato momento em que o evento acontece (ver LivroService/StreakService).
/// DesafioConcluido é o único que NUNCA é persistido: em vez de guardar
/// uma linha quando o desafio termina (o que exigiria um job rodando em
/// segundo plano pra disparar algo exatamente na DataFim — algo que não
/// existe em nenhum outro lugar deste projeto), o FeedService calcula esse
/// evento na hora de montar o feed, comparando DataFim de cada desafio com
/// a data atual. O enum existe mesmo assim porque os dois casos (persistido
/// e calculado) viram o mesmo tipo de "linha" no feed — só a origem do dado
/// é diferente.
/// </summary>
public enum TipoAtividade
{
    TerminouLivro = 0,
    ResenhaPublicada = 1,
    MetaBatida = 2,
    CheckIn = 3,
    DesafioConcluido = 4
}

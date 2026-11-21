using EstanteDigital.Domain.Enums;

namespace EstanteDigital.Domain.Entidades;

/// <summary>
/// Um evento do feed de atividade, registrado no momento em que acontece
/// (ver TipoAtividade para o porquê de DesafioConcluido nunca aparecer
/// aqui). Os campos opcionais (LivroId, MetaValor, SequenciaDias) só fazem
/// sentido dependendo de Tipo — não é o design mais "normalizado" possível
/// (um enum + campos nem sempre preenchidos), mas evita criar uma tabela
/// por tipo de evento pra um recurso que é, no fundo, só uma linha do tempo.
/// </summary>
public class Atividade
{
    public Guid Id { get; set; }

    /// <summary>De quem é a atividade (quem terminou o livro, quem
    /// registrou o check-in, etc.) — o feed de outra pessoa mostra isto
    /// filtrado por quem ELA segue (ver FeedService).</summary>
    public Guid UsuarioId { get; set; }

    public TipoAtividade Tipo { get; set; }

    /// <summary>Preenchido em TerminouLivro/ResenhaPublicada.</summary>
    public Guid? LivroId { get; set; }

    /// <summary>Preenchido em MetaBatida: o valor da meta atingida naquele momento.</summary>
    public int? MetaValor { get; set; }

    /// <summary>Preenchido em CheckIn: a sequência de dias já alcançada
    /// incluindo este check-in.</summary>
    public int? SequenciaDias { get; set; }

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}

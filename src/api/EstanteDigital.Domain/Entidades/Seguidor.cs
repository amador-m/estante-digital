namespace EstanteDigital.Domain.Entidades;

/// <summary>
/// Representa "SeguidorId segue SeguidoId" — uma relação de mão única (seguir
/// não precisa ser recíproco, como no Instagram/Twitter, diferente de uma
/// "amizade" do Facebook que seria nos dois sentidos). Não existe conceito
/// de "aceitar pedido" aqui: seguir é imediato, de propósito — simples o
/// suficiente pra um projeto de portfólio, sem a complexidade extra de
/// pedidos pendentes/aprovação que um app de rede social real teria.
/// </summary>
public class Seguidor
{
    public Guid Id { get; set; }

    /// <summary>Quem decidiu seguir.</summary>
    public Guid SeguidorId { get; set; }

    /// <summary>Quem está sendo seguido.</summary>
    public Guid SeguidoId { get; set; }

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}

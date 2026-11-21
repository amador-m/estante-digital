namespace EstanteDigital.Domain.Entidades;

/// <summary>
/// Dono de uma estante. Cada Livro pertence a exatamente um Usuario
/// (ver Livro.UsuarioId) — é isso que torna o acervo "privado por pessoa"
/// em vez do catálogo único e público das versões anteriores deste projeto.
/// </summary>
public class Usuario
{
    public Guid Id { get; set; }

    /// <summary>Sempre guardado em minúsculas (ver AuthService) — e-mail não é
    /// case-sensitive na prática, e duas contas "Maria@x.com"/"maria@x.com"
    /// não podem coexistir.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Nunca a senha em si — só o hash (PBKDF2, ver Infrastructure/Auth/SenhaHasher).
    /// O formato guardado aqui já inclui o salt e o número de iterações,
    /// então não existe "SenhaSalt" separado.
    /// </summary>
    public string SenhaHash { get; set; } = string.Empty;

    /// <summary>Nome de exibição único (diferente do e-mail) — aparece na
    /// interface no lugar do e-mail cru. Gerado automaticamente a partir do
    /// e-mail no cadastro (ver AuthService); a pessoa troca depois em "Perfil".</summary>
    public string NomeUsuario { get; set; } = string.Empty;

    /// <summary>Chave de um ícone fixo (ver Application.Constantes.OpcoesPerfil.Icones) —
    /// nunca um upload de imagem livre, de propósito: sem arquivo pra
    /// gerenciar, sem risco de upload impróprio.</summary>
    public string Icone { get; set; } = "livro-aberto";

    /// <summary>Chave de uma cor fixa (ver Application.Constantes.OpcoesPerfil.Cores) —
    /// a cor de destaque (botões, links, logo) em toda a interface desta conta.</summary>
    public string CorPrincipal { get; set; } = "azul";

    /// <summary>Quantos livros a pessoa quer terminar de ler no ano corrente —
    /// simples de propósito (não guarda histórico por ano; se a pessoa quiser
    /// mudar a meta no meio do ano, ou reaproveitar pro ano seguinte, edita
    /// o mesmo número em "Perfil"). O progresso é calculado contando
    /// Livro.DataConclusao dentro do ano corrente (ver LivroService).</summary>
    public int? MetaLeituraAnual { get; set; }

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}

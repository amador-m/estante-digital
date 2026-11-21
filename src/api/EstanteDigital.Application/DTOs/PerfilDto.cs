namespace EstanteDigital.Application.DTOs;

public record PerfilDto(
    Guid Id,
    string Email,
    string NomeUsuario,
    string Icone,
    string CorPrincipal,
    int? MetaLeituraAnual,
    DateTime CriadoEm
);

public record AtualizarPerfilDto(string NomeUsuario, string Icone, string CorPrincipal, int? MetaLeituraAnual);

public record AlterarSenhaDto(string SenhaAtual, string NovaSenha);

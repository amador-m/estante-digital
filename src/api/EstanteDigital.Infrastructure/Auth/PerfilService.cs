using System.Text.RegularExpressions;
using EstanteDigital.Application.Constantes;
using EstanteDigital.Application.DTOs;
using EstanteDigital.Application.Excecoes;
using EstanteDigital.Application.Interfaces;
using EstanteDigital.Domain.Entidades;
using EstanteDigital.Domain.Repositorios;

namespace EstanteDigital.Infrastructure.Auth;

/// <summary>
/// Fica em Infrastructure (não em Application), mesmo padrão já usado em
/// AuthService: depende de SenhaHasher e de IUsuarioRepository, que são
/// detalhes de infraestrutura — Application só conhece a interface
/// (IPerfilService) e os DTOs.
/// </summary>
public class PerfilService : IPerfilService
{
    private static readonly Regex FormatoNomeUsuario = new(@"^[a-zA-Z0-9._-]+$", RegexOptions.Compiled);

    private readonly IUsuarioRepository _usuarioRepository;

    public PerfilService(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    public async Task<PerfilDto?> ObterAsync(Guid usuarioId)
    {
        var usuario = await _usuarioRepository.BuscarPorIdAsync(usuarioId);
        return usuario is null ? null : ParaDto(usuario);
    }

    public async Task<PerfilDto> AtualizarAsync(Guid usuarioId, AtualizarPerfilDto dto)
    {
        var usuario = await _usuarioRepository.BuscarPorIdAsync(usuarioId)
            ?? throw new ValidacaoException("Usuário não encontrado.");

        var nomeUsuario = dto.NomeUsuario.Trim();
        if (nomeUsuario.Length < 3 || nomeUsuario.Length > 24)
        {
            throw new ValidacaoException("O nome de usuário deve ter entre 3 e 24 caracteres.");
        }

        if (!FormatoNomeUsuario.IsMatch(nomeUsuario))
        {
            throw new ValidacaoException("O nome de usuário só pode ter letras, números, ponto, traço e underline.");
        }

        var existente = await _usuarioRepository.BuscarPorNomeUsuarioAsync(nomeUsuario);
        if (existente is not null && existente.Id != usuarioId)
        {
            throw new ValidacaoException("Este nome de usuário já está em uso.");
        }

        if (!OpcoesPerfil.Icones.Contains(dto.Icone))
        {
            throw new ValidacaoException($"Ícone inválido. Valores aceitos: {string.Join(", ", OpcoesPerfil.Icones)}.");
        }

        if (!OpcoesPerfil.Cores.Contains(dto.CorPrincipal))
        {
            throw new ValidacaoException($"Cor inválida. Valores aceitos: {string.Join(", ", OpcoesPerfil.Cores)}.");
        }

        if (dto.MetaLeituraAnual is < 0 or > 1000)
        {
            throw new ValidacaoException("A meta de leitura anual deve ser um número entre 0 e 1000.");
        }

        usuario.NomeUsuario = nomeUsuario;
        usuario.Icone = dto.Icone;
        usuario.CorPrincipal = dto.CorPrincipal;
        usuario.MetaLeituraAnual = dto.MetaLeituraAnual;

        var atualizado = await _usuarioRepository.AtualizarAsync(usuario);
        return ParaDto(atualizado);
    }

    public async Task AlterarSenhaAsync(Guid usuarioId, AlterarSenhaDto dto)
    {
        var usuario = await _usuarioRepository.BuscarPorIdAsync(usuarioId)
            ?? throw new ValidacaoException("Usuário não encontrado.");

        if (!SenhaHasher.Verificar(dto.SenhaAtual, usuario.SenhaHash))
        {
            throw new ValidacaoException("Senha atual incorreta.");
        }

        if (string.IsNullOrWhiteSpace(dto.NovaSenha) || dto.NovaSenha.Length < 6)
        {
            throw new ValidacaoException("A nova senha deve ter pelo menos 6 caracteres.");
        }

        usuario.SenhaHash = SenhaHasher.Hash(dto.NovaSenha);
        await _usuarioRepository.AtualizarAsync(usuario);
    }

    private static PerfilDto ParaDto(Usuario usuario) => new(
        usuario.Id,
        usuario.Email,
        usuario.NomeUsuario,
        usuario.Icone,
        usuario.CorPrincipal,
        usuario.MetaLeituraAnual,
        usuario.CriadoEm
    );
}

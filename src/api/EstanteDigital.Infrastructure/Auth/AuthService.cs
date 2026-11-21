using System.Text.RegularExpressions;
using EstanteDigital.Application.Constantes;
using EstanteDigital.Application.DTOs;
using EstanteDigital.Application.Excecoes;
using EstanteDigital.Application.Interfaces;
using EstanteDigital.Domain.Entidades;
using EstanteDigital.Domain.Repositorios;

namespace EstanteDigital.Infrastructure.Auth;

public class AuthService : IAuthService
{
    private static readonly Regex FormatoEmail = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    private readonly IUsuarioRepository _usuarioRepository;
    private readonly TokenService _tokenService;
    private readonly RefreshTokenStore _refreshTokenStore;

    public AuthService(IUsuarioRepository usuarioRepository, TokenService tokenService, RefreshTokenStore refreshTokenStore)
    {
        _usuarioRepository = usuarioRepository;
        _tokenService = tokenService;
        _refreshTokenStore = refreshTokenStore;
    }

    public async Task<TokenResponseDto> RegistrarAsync(RegistroDto dto)
    {
        var email = dto.Email.Trim();

        if (!FormatoEmail.IsMatch(email))
        {
            throw new ValidacaoException("Informe um e-mail válido.");
        }

        if (string.IsNullOrWhiteSpace(dto.Senha) || dto.Senha.Length < 6)
        {
            throw new ValidacaoException("A senha deve ter pelo menos 6 caracteres.");
        }

        var existente = await _usuarioRepository.BuscarPorEmailAsync(email);
        if (existente is not null)
        {
            throw new ValidacaoException("Este e-mail já está cadastrado.");
        }

        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Email = email,
            SenhaHash = SenhaHasher.Hash(dto.Senha),
            NomeUsuario = await GerarNomeUsuarioUnicoAsync(email),
            Icone = OpcoesPerfil.IconePadrao,
            CorPrincipal = OpcoesPerfil.CorPadrao,
            CriadoEm = DateTime.UtcNow
        };

        await _usuarioRepository.AdicionarAsync(usuario);

        // Já devolve os tokens — não faz sentido pedir pra pessoa logar de
        // novo imediatamente depois de criar a conta.
        return GerarTokens(usuario);
    }

    /// <summary>
    /// O cadastro só pede e-mail/senha (não forçamos mais um campo no
    /// formulário de criar conta) — então o nome de usuário inicial é
    /// derivado da parte antes do "@" do e-mail, e a pessoa troca depois em
    /// "Perfil" se quiser. Como NomeUsuario precisa ser único, acrescenta um
    /// sufixo numérico quando já existe alguém com o mesmo nome base.
    /// </summary>
    private async Task<string> GerarNomeUsuarioUnicoAsync(string email)
    {
        var baseNome = Regex.Replace(email.Split('@')[0].ToLowerInvariant(), @"[^a-z0-9._-]", "");
        if (string.IsNullOrWhiteSpace(baseNome))
        {
            baseNome = "leitor";
        }

        var candidato = baseNome;
        var sufixo = 1;
        while (await _usuarioRepository.BuscarPorNomeUsuarioAsync(candidato) is not null)
        {
            sufixo++;
            candidato = $"{baseNome}{sufixo}";
        }

        return candidato;
    }

    public async Task<TokenResponseDto?> LoginAsync(LoginDto dto)
    {
        var usuario = await _usuarioRepository.BuscarPorEmailAsync(dto.Email.Trim());
        if (usuario is null || !SenhaHasher.Verificar(dto.Senha, usuario.SenhaHash))
        {
            return null;
        }

        return GerarTokens(usuario);
    }

    public async Task<TokenResponseDto?> RefreshAsync(string refreshToken)
    {
        var usuarioId = _refreshTokenStore.ValidarEUsar(refreshToken);
        if (usuarioId is null)
        {
            return null;
        }

        var usuario = await _usuarioRepository.BuscarPorIdAsync(usuarioId.Value);
        if (usuario is null)
        {
            return null;
        }

        return GerarTokens(usuario); // rotação: token antigo já foi consumido acima
    }

    public Task LogoutAsync(string refreshToken)
    {
        _refreshTokenStore.Revogar(refreshToken);
        return Task.CompletedTask;
    }

    private TokenResponseDto GerarTokens(Usuario usuario)
    {
        var accessToken = _tokenService.GerarAccessToken(usuario.Id, usuario.Email);
        var refreshToken = _refreshTokenStore.Gerar(usuario.Id);
        return new TokenResponseDto(accessToken, refreshToken);
    }
}

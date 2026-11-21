using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EstanteDigital.Infrastructure.Auth;

/// <summary>
/// Substitui o middleware oficial Microsoft.AspNetCore.Authentication.JwtBearer
/// (pacote NuGet, bloqueado no sandbox). Lê o header "Authorization: Bearer
/// &lt;token&gt;", valida com o TokenService manual e monta o ClaimsPrincipal —
/// é o que faz o [Authorize] funcionar nos controllers sem precisar do
/// pacote oficial.
/// </summary>
public class BearerAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly TokenService _tokenService;

    public BearerAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        TokenService tokenService)
        : base(options, logger, encoder)
    {
        _tokenService = tokenService;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var cabecalho))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var valor = cabecalho.ToString();
        if (!valor.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var token = valor["Bearer ".Length..].Trim();
        var claims = _tokenService.ValidarToken(token);

        if (claims is null)
        {
            return Task.FromResult(AuthenticateResult.Fail("Token inválido ou expirado."));
        }

        var identidade = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Email, claims["email"].ToString() ?? string.Empty),
            // NameIdentifier carrega o id do usuário (claim "sub" do token) —
            // é isso que todo controller usa pra saber de quem é a estante
            // (User.FindFirstValue(ClaimTypes.NameIdentifier)), nunca o e-mail.
            new Claim(ClaimTypes.NameIdentifier, claims["sub"].ToString() ?? string.Empty)
        }, Scheme.Name);

        var principal = new ClaimsPrincipal(identidade);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

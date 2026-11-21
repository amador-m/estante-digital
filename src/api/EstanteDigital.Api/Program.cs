using EstanteDigital.Application.Interfaces;
using EstanteDigital.Application.Servicos;
using EstanteDigital.Domain.Repositorios;
using EstanteDigital.Infrastructure.Auth;
using EstanteDigital.Infrastructure.Persistencia;
#if USAR_POSTGRES
using EstanteDigital.Infrastructure.Persistencia.Ef;
using Microsoft.EntityFrameworkCore;
#endif
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// --- CORS: catálogo é público, mas ainda assim só queremos aceitar
// requisições vindas do nosso próprio front (dev e Docker) ---
var origensPermitidas = builder.Configuration.GetSection("Cors:OrigensPermitidas").Get<string[]>()
    ?? new[] { "http://localhost:5173" };

const string PoliticaFrontend = "FrontendPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(PoliticaFrontend, policy =>
    {
        policy.WithOrigins(origensPermitidas).AllowAnyHeader().AllowAnyMethod();
    });
});

// --- Domain/Application/Infrastructure: registrados aqui no Program.cs
// porque é o único projeto com referência ao ASP.NET Core/Microsoft.Extensions.*
// necessária para IServiceCollection/IConfiguration (ver csproj do
// Infrastructure para o porquê de ele também precisar de FrameworkReference) ---

var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()
    ?? throw new InvalidOperationException("Seção 'Jwt' ausente em appsettings.json.");
builder.Services.AddSingleton(jwtOptions);
builder.Services.AddSingleton<TokenService>();
builder.Services.AddSingleton<RefreshTokenStore>();

#if USAR_POSTGRES
// --- Persistência real: PostgreSQL via EF Core ---
// Só é compilado quando o projeto é construído com `-p:UsarPostgres=true`
// (o Dockerfile da API faz isso) — ver o comentário no .csproj do
// Infrastructure e em docs/ARQUITETURA.md para o porquê.
var connectionString = builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException("ConnectionStrings:Postgres ausente em appsettings.json.");
builder.Services.AddDbContext<EstanteDigitalDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddScoped<ILivroRepository, LivroRepositorioEfCore>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepositorioEfCore>();
builder.Services.AddScoped<ISeguidorRepository, SeguidorRepositorioEfCore>();
builder.Services.AddScoped<IDesafioRepository, DesafioRepositorioEfCore>();
builder.Services.AddScoped<IParticipacaoDesafioRepository, ParticipacaoDesafioRepositorioEfCore>();
builder.Services.AddScoped<ICheckInRepository, CheckInRepositorioEfCore>();
builder.Services.AddScoped<IAtividadeRepository, AtividadeRepositorioEfCore>();
// DbContext é Scoped (nunca pode ser Singleton — não é thread-safe), então
// tudo que depende dele, direta ou indiretamente, também precisa ser
// Scoped — por isso IAuthService muda de Singleton (variante JSON, abaixo)
// para Scoped só nesta variante.
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPerfilService, PerfilService>();
#else
// --- Persistência testável por mim: arquivo JSON ---
var caminhoArquivoLivros = Path.Combine(
    AppContext.BaseDirectory,
    builder.Configuration["Armazenamento:CaminhoArquivoLivros"] ?? "Dados/livros.json");
var caminhoArquivoUsuarios = Path.Combine(
    AppContext.BaseDirectory,
    builder.Configuration["Armazenamento:CaminhoArquivoUsuarios"] ?? "Dados/usuarios.json");
var caminhoArquivoSeguidores = Path.Combine(
    AppContext.BaseDirectory,
    builder.Configuration["Armazenamento:CaminhoArquivoSeguidores"] ?? "Dados/seguidores.json");
var caminhoArquivoDesafios = Path.Combine(
    AppContext.BaseDirectory,
    builder.Configuration["Armazenamento:CaminhoArquivoDesafios"] ?? "Dados/desafios.json");
var caminhoArquivoParticipacoesDesafio = Path.Combine(
    AppContext.BaseDirectory,
    builder.Configuration["Armazenamento:CaminhoArquivoParticipacoesDesafio"] ?? "Dados/participacoes_desafio.json");
var caminhoArquivoCheckIns = Path.Combine(
    AppContext.BaseDirectory,
    builder.Configuration["Armazenamento:CaminhoArquivoCheckIns"] ?? "Dados/checkins_leitura.json");
var caminhoArquivoAtividades = Path.Combine(
    AppContext.BaseDirectory,
    builder.Configuration["Armazenamento:CaminhoArquivoAtividades"] ?? "Dados/atividades.json");

// Todos Singleton (não Scoped): são repositórios baseados em arquivo JSON
// com sua própria trava (SemaphoreSlim) para serializar escritas
// concorrentes — isso só funciona se for a MESMA instância compartilhada
// entre todas as requisições.
builder.Services.AddSingleton<ILivroRepository>(_ =>
    new ArquivoJsonLivroRepository(caminhoArquivoLivros, DadosIniciais.ObterCatalogoInicial()));
builder.Services.AddSingleton<IUsuarioRepository>(_ =>
    new UsuarioJsonRepository(caminhoArquivoUsuarios, DadosIniciais.ObterUsuarioDemo()));
builder.Services.AddSingleton<ISeguidorRepository>(_ =>
    new SeguidorJsonRepository(caminhoArquivoSeguidores));
builder.Services.AddSingleton<IDesafioRepository>(_ =>
    new DesafioJsonRepository(caminhoArquivoDesafios));
builder.Services.AddSingleton<IParticipacaoDesafioRepository>(_ =>
    new ParticipacaoDesafioJsonRepository(caminhoArquivoParticipacoesDesafio));
builder.Services.AddSingleton<ICheckInRepository>(_ =>
    new CheckInJsonRepository(caminhoArquivoCheckIns));
builder.Services.AddSingleton<IAtividadeRepository>(_ =>
    new AtividadeJsonRepository(caminhoArquivoAtividades));

builder.Services.AddSingleton<IAuthService, AuthService>();
builder.Services.AddSingleton<IPerfilService, PerfilService>();
#endif

builder.Services.AddScoped<ILivroService, LivroService>();
// Scoped funciona nas duas variantes: tanto dependendo dos repositórios
// Scoped (EF Core) quanto dos Singleton (JSON) — Scoped-depender-de-Singleton
// é sempre uma combinação válida no DI do ASP.NET Core.
builder.Services.AddScoped<IPerfilSocialService, PerfilSocialService>();
builder.Services.AddScoped<IDesafioService, DesafioService>();
builder.Services.AddScoped<IStreakService, StreakService>();
builder.Services.AddScoped<IFeedService, FeedService>();

builder.Services
    .AddAuthentication("Bearer")
    .AddScheme<AuthenticationSchemeOptions, BearerAuthenticationHandler>("Bearer", _ => { });
builder.Services.AddAuthorization();

var app = builder.Build();

#if USAR_POSTGRES
// Cria o schema a partir do modelo (sem pasta de Migrations — EnsureCreated
// em vez de Migrate/dotnet-ef, ver docs/ARQUITETURA.md) e, se o banco ainda
// estiver vazio, semeia
// a conta demo + o catálogo de exemplo — assim quem sobe o projeto do zero
// já consegue logar e ver uma estante preenchida.
using (var escopoInicial = app.Services.CreateScope())
{
    var db = escopoInicial.ServiceProvider.GetRequiredService<EstanteDigitalDbContext>();
    db.Database.EnsureCreated();

    if (!db.Usuarios.Any())
    {
        db.Usuarios.AddRange(DadosIniciais.ObterUsuarioDemo());
        db.Livros.AddRange(DadosIniciais.ObterCatalogoInicial());
        db.SaveChanges();
    }
}
#endif

// Ordem importa: CORS antes de tudo, Authentication antes de Authorization.
app.UseCors(PoliticaFrontend);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health check simples, usado na demonstração e pelo healthcheck do Docker.
app.MapGet("/health", () => Results.Ok(new { status = "ok", timestampUtc = DateTime.UtcNow }))
    .AllowAnonymous();

app.Run();

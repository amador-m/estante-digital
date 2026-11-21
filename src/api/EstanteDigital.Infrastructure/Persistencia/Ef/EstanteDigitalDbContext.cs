#if USAR_POSTGRES
using EstanteDigital.Domain.Entidades;
using Microsoft.EntityFrameworkCore;

namespace EstanteDigital.Infrastructure.Persistencia.Ef;

/// <summary>
/// DbContext real, com PostgreSQL via Npgsql. Este arquivo inteiro só é
/// compilado quando o projeto é construído com `-p:UsarPostgres=true` (ver
/// Dockerfile da API) — é a única forma que encontrei de escrever a
/// persistência "de verdade" que você pediu (banco relacional) sem quebrar
/// o build do projeto inteiro no meu ambiente de trabalho, onde os pacotes
/// do EF Core/Npgsql não podem ser baixados do NuGet (mesmo bloqueio que já
/// afetava o Docker — ver LEIA-ME_Aula20.md, seção Fase 2). Em compensação:
/// eu não consegui compilar nem rodar esta classe especificamente — só a
/// escrevi com cuidado, seguindo o jeito padrão do EF Core. É a parte que
/// peço pra você validar rodando o `docker compose up --build`.
/// </summary>
public class EstanteDigitalDbContext : DbContext
{
    public EstanteDigitalDbContext(DbContextOptions<EstanteDigitalDbContext> options) : base(options)
    {
    }

    public DbSet<Livro> Livros => Set<Livro>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Seguidor> Seguidores => Set<Seguidor>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(usuario =>
        {
            usuario.ToTable("usuarios");
            usuario.HasKey(u => u.Id);
            usuario.Property(u => u.Email).IsRequired().HasMaxLength(256);
            // Índice único no e-mail — a mesma regra que AuthService já
            // aplica na camada de aplicação (BuscarPorEmailAsync antes de
            // cadastrar), só que também garantida pelo próprio banco, caso
            // dois cadastros cheguem ao mesmo tempo (condição de corrida
            // que a checagem em memória sozinha não cobre).
            usuario.HasIndex(u => u.Email).IsUnique();
            usuario.Property(u => u.SenhaHash).IsRequired();
            usuario.Property(u => u.NomeUsuario).IsRequired().HasMaxLength(24);
            usuario.HasIndex(u => u.NomeUsuario).IsUnique();
            usuario.Property(u => u.Icone).IsRequired().HasMaxLength(30);
            usuario.Property(u => u.CorPrincipal).IsRequired().HasMaxLength(20);
        });

        modelBuilder.Entity<Livro>(livro =>
        {
            livro.ToTable("livros");
            livro.HasKey(l => l.Id);
            livro.Property(l => l.Titulo).IsRequired().HasMaxLength(300);
            livro.Property(l => l.Autor).IsRequired().HasMaxLength(200);
            livro.Property(l => l.Serie).HasMaxLength(200);
            livro.Property(l => l.SinopseCurta).IsRequired();
            livro.Property(l => l.Resenha).HasMaxLength(4000);

            // Enum guardado como texto ("Fantasia", "Lido"...) em vez de
            // número — mais legível direto no banco (um `SELECT *` já
            // mostra o valor, sem precisar decorar a ordem do enum).
            livro.Property(l => l.Genero).HasConversion<string>().HasMaxLength(30);
            livro.Property(l => l.StatusLeitura).HasConversion<string>().HasMaxLength(30);

            // "Nasce filtrado" começa aqui: índice em UsuarioId porque toda
            // consulta de livro passa por ele (ver LivroRepositorioEfCore).
            livro.HasIndex(l => l.UsuarioId);

            livro.HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(l => l.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Seguidor>(seguidor =>
        {
            seguidor.ToTable("seguidores");
            seguidor.HasKey(s => s.Id);

            // Único índice composto: a mesma dupla (SeguidorId, SeguidoId) não
            // pode existir duas vezes — é o banco garantindo a idempotência
            // que o SeguidorJsonRepository já garante em memória.
            seguidor.HasIndex(s => new { s.SeguidorId, s.SeguidoId }).IsUnique();

            seguidor.HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(s => s.SeguidorId)
                .OnDelete(DeleteBehavior.Cascade);

            // Restrict (não Cascade) neste segundo FK: sem isso, o EF Core
            // reclama de "múltiplos caminhos de cascade" entre Usuario e
            // Seguidor (duas FKs pra Usuario na mesma tabela) — e mesmo sem
            // esse erro, faria sentido mesmo: apagar a conta de quem é
            // seguido não deveria apagar em cascata a lista de quem segue
            // outras pessoas.
            seguidor.HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(s => s.SeguidoId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
#endif

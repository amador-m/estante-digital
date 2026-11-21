# Arquitetura — Estante Digital

Resumo pensado pro "Pitch Técnico" (Aula 20): o que foi construído, e por quê, sem enrolação — pra você conseguir explicar isso em 2-3 frases numa entrevista ou numa demonstração.

## Visão geral

```
┌──────────────────┐        JSON / JWT        ┌───────────────────────────────────┐
│   React + Vite    │ ───────────────────────▶ │           ASP.NET Core             │
│  (porta 5173/8080)│ ◀─────────────────────── │           (porta 5230)             │
└──────────────────┘                          └───────────────────────────────────┘
                                                              │
                                                 ┌────────────┴────────────┐
                                                 │   Clean Architecture    │
                                                 │  (4 projetos, 1 sentido │
                                                 │   de dependência só)    │
                                                 └──────────────────────────┘
```

## As 4 camadas do backend e a regra de dependência

```
EstanteDigital.Api
       │  depende de
       ▼
EstanteDigital.Infrastructure ──┐
       │  depende de            │ depende de
       ▼                        ▼
EstanteDigital.Application ──▶ EstanteDigital.Domain
```

A seta sempre aponta **para dentro** — ninguém depende de quem está mais externo. `Domain` não conhece nenhuma das outras três; é só `class Livro { ... }` e as *interfaces* (`ILivroRepository`) que as camadas de fora implementam. Essa inversão é o motivo de existir Clean Architecture: o "Livro" e as regras de negócio sobre ele não mudam se amanhã você trocar o arquivo JSON por PostgreSQL, ou o ASP.NET Core por outra coisa — só a `Infrastructure` muda.

- **`Domain`** — `Livro` e `Usuario` (entidades — todo `Livro` carrega um `UsuarioId`, o dono da estante), `Seguidor` (relação de "seguir" entre duas contas, uma via só — não é amizade mútua), `GeneroLivro`/`StatusLeitura` (enums), `ILivroRepository`/`IUsuarioRepository`/`ISeguidorRepository` (contratos, sem implementação). Não referencia nenhum outro projeto.
- **`Application`** — DTOs (o que entra e sai da API), `LivroService` (as regras: validação de título/autor/ano/sinopse, regra "só avalia quem já foi lido", filtros do catálogo, agregação do dashboard — tudo já recebendo o `usuarioId` de quem está logado), `PerfilSocialService` (busca de usuários, seguir/deixar de seguir, montagem do perfil público com a regra de visibilidade em duas camadas — ver `LEIA-ME_Aula20.md`, seção Fase 3), interfaces de serviço (`ILivroService`, `IAuthService`, `IPerfilSocialService`). Depende só de `Domain`.
- **`Infrastructure`** — tudo que é "detalhe técnico": os repositórios concretos (hoje em **duas** variantes — ver seção "Persistência" abaixo), `SenhaHasher` (hash de senha PBKDF2), `TokenService`/`RefreshTokenStore`/`BearerAuthenticationHandler` (o JWT manual, herdado da Aula 12/16, sem mais o conceito de "Role"). Depende de `Domain` + `Application`, e também referencia o framework do ASP.NET Core (`FrameworkReference`) porque é aqui que vive o `AuthenticationHandler`.
- **`Api`** — só Controllers (fininhos: recebem DTO, chamam o Service, devolvem o resultado — nenhuma regra de negócio mora aqui) e `Program.cs` (onde tudo é "plugado" via injeção de dependência).

## Persistência: PostgreSQL (real) + arquivo JSON (meu caminho de teste)

A persistência definitiva deste projeto é **PostgreSQL via EF Core** (`Infrastructure/Persistencia/Ef/`) — é o que roda no `docker compose up`. Ela convive no código com a implementação anterior em arquivo JSON (`Infrastructure/Persistencia/ArquivoJsonLivroRepository.cs`/`UsuarioJsonRepository.cs`), e as duas implementam exatamente o mesmo contrato (`ILivroRepository`/`IUsuarioRepository`) — é o próprio Clean Architecture permitindo essa troca sem tocar em `Domain`, `Application` ou nos Controllers.

O motivo de as duas existirem ao mesmo tempo não é uma recomendação geral de arquitetura — é uma resposta a uma limitação de ambiente: quem escreveu este código (a IA, durante a tutoria) trabalha num sandbox que bloqueia o download de pacotes do NuGet, então os pacotes do EF Core/Npgsql não podiam nem ser restaurados ali, muito menos compilados ou testados. A solução foi uma propriedade de build (`UsarPostgres`, nos `.csproj` de `Infrastructure` e `Api`): por padrão ela é `false`, os arquivos de `Persistencia/Ef/` ficam fora da compilação (via `#if USAR_POSTGRES`) e o projeto roda com o repositório em JSON — é o que permitiu testar toda a lógica de negócio de ponta a ponta fora do Docker. O `Dockerfile` da API é quem builda com `-p:UsarPostgres=true`, ligando a persistência real. Detalhes completos (inclusive o que ficou sem teste) estão no `LEIA-ME_Aula20.md`, seção "Fase 2".

## Autenticação

JWT manual (HMAC-SHA256, sem pacote de terceiros) com refresh token rotativo, mesma base das Aulas 12 e 16 — ver `EstanteDigital.Infrastructure/Auth/`. Diferente da Aula 16, aqui **não existe mais o conceito de "Role"**: cada conta logada só gerencia a própria estante (modelo multi-tenant — toda consulta de `Livro` é filtrada por `UsuarioId`), então não há mais uma distinção "Admin × usuário comum" com propósito real. A claim que importa no token é `NameIdentifier` (o id da conta), não um papel. Senhas são armazenadas com hash PBKDF2-HMACSHA256 (`SenhaHasher`), nunca em texto puro.

## Docker

Três containers: banco (`postgres:16-alpine`, imagem oficial, sem Dockerfile próprio), backend e frontend — os dois últimos com Dockerfile multi-stage (compila numa imagem maior, roda numa imagem final enxuta). O frontend é servido por Nginx como arquivos estáticos — não existe um "servidor Node" rodando em produção. Detalhe que costuma pegar quem não fez isso antes: a variável `VITE_API_URL` é "queimada" dentro do JavaScript **no momento do build da imagem**, não em tempo de execução — por isso ela é um `ARG` do Dockerfile, e aponta para a porta que a API expõe no seu PC (quem faz a chamada é o navegador, não outro container).

A API só sobe depois que o Postgres responde de verdade a conexões (`depends_on: db: condition: service_healthy`, usando `pg_isready` como healthcheck) — sem isso, a primeira tentativa de conexão da API numa subida a frio poderia falhar por pura corrida entre os dois containers.

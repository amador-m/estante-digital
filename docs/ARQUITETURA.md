# Arquitetura - Estante Digital

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

A seta sempre aponta **para dentro**, ninguém depende de quem está mais externo. `Domain` não conhece nenhuma das outras três; é só `class Livro { ... }` e as *interfaces* (`ILivroRepository`) que as camadas de fora implementam. Essa inversão é o motivo de existir Clean Architecture: o "Livro" e as regras de negócio sobre ele não mudam se amanhã você trocar o arquivo JSON por PostgreSQL, ou o ASP.NET Core por outra coisa — só a `Infrastructure` muda.

- **`Domain`** — `Livro` e `Usuario` (entidades — todo `Livro` carrega um `UsuarioId`, o dono da estante), `Seguidor` (relação de "seguir" entre duas contas, uma via só — não é amizade mútua), `GeneroLivro`/`StatusLeitura` (enums), `ILivroRepository`/`IUsuarioRepository`/`ISeguidorRepository` (contratos, sem implementação). Não referencia nenhum outro projeto.
- **`Application`** — DTOs (o que entra e sai da API), `LivroService` (as regras: validação de título/autor/ano/sinopse, regra "só avalia quem já foi lido", filtros do catálogo, agregação do dashboard — tudo já recebendo o `usuarioId` de quem está logado), `PerfilSocialService` (busca de usuários, seguir/deixar de seguir, montagem do perfil público com a regra de visibilidade em duas camadas: estatísticas sempre visíveis, estante completa só pra quem segue), interfaces de serviço (`ILivroService`, `IAuthService`, `IPerfilSocialService`). Depende só de `Domain`.
- **`Infrastructure`** — tudo que é "detalhe técnico": os repositórios concretos (hoje em **duas** variantes — ver seção "Persistência" abaixo), `SenhaHasher` (hash de senha PBKDF2), `TokenService`/`RefreshTokenStore`/`BearerAuthenticationHandler` (o JWT manual, herdado da Aula 12/16, sem mais o conceito de "Role"). Depende de `Domain` + `Application`, e também referencia o framework do ASP.NET Core (`FrameworkReference`) porque é aqui que vive o `AuthenticationHandler`.
- **`Api`** — só Controllers (fininhos: recebem DTO, chamam o Service, devolvem o resultado — nenhuma regra de negócio mora aqui) e `Program.cs` (onde tudo é "plugado" via injeção de dependência).

## Persistência: PostgreSQL (real) + arquivo JSON (meu caminho de teste)

A persistência definitiva deste projeto é **PostgreSQL via EF Core** (`Infrastructure/Persistencia/Ef/`) — é o que roda no `docker compose up`. Ela convive no código com a implementação anterior em arquivo JSON (`Infrastructure/Persistencia/ArquivoJsonLivroRepository.cs`/`UsuarioJsonRepository.cs`), e as duas implementam exatamente o mesmo contrato (`ILivroRepository`/`IUsuarioRepository`) — é o próprio Clean Architecture permitindo essa troca sem tocar em `Domain`, `Application` ou nos Controllers.

O motivo de as duas existirem ao mesmo tempo não é uma recomendação geral de arquitetura — é uma resposta a uma limitação de ambiente: quem escreveu este código (a IA, durante a tutoria) trabalha num sandbox que bloqueia o download de pacotes do NuGet, então os pacotes do EF Core/Npgsql não podiam nem ser restaurados ali, muito menos compilados ou testados. A solução foi uma propriedade de build (`UsarPostgres`, nos `.csproj` de `Infrastructure` e `Api`): por padrão ela é `false`, os arquivos de `Persistencia/Ef/` ficam fora da compilação (via `#if USAR_POSTGRES`) e o projeto roda com o repositório em JSON — é o que permitiu testar toda a lógica de negócio de ponta a ponta fora do Docker. O `Dockerfile` da API é quem builda com `-p:UsarPostgres=true`, ligando a persistência real. A parte em `Persistencia/Ef/` não pôde ser compilada nem executada no ambiente onde o projeto foi desenvolvido (mesmo bloqueio de NuGet) — só a versão em JSON foi testada de ponta a ponta ali; a versão Postgres é validada rodando `docker compose up --build`.

## Autenticação

JWT manual (HMAC-SHA256, sem pacote de terceiros) com refresh token rotativo, mesma base das Aulas 12 e 16 — ver `EstanteDigital.Infrastructure/Auth/`. Diferente da Aula 16, aqui **não existe mais o conceito de "Role"**: cada conta logada só gerencia a própria estante (modelo multi-tenant — toda consulta de `Livro` é filtrada por `UsuarioId`), então não há mais uma distinção "Admin × usuário comum" com propósito real. A claim que importa no token é `NameIdentifier` (o id da conta), não um papel. Senhas são armazenadas com hash PBKDF2-HMACSHA256 (`SenhaHasher`), nunca em texto puro.

## Fase 4 — Desafios, streak e feed

Três ideias novas (`Desafio`/`ParticipacaoDesafio`, `CheckInLeitura`, `Atividade`), cada uma com seu serviço em `Application` (`DesafioService`, `StreakService`, `FeedService`) e seus repositórios em `Infrastructure` (JSON + EF Core, mesmo padrão das camadas anteriores). Alguns pontos que não vieram explicitamente pedidos e valem registro, pro caso de precisar justificar ou desfazer depois:

- **Participação é aberta** — qualquer conta entra em qualquer desafio, sem convite nem aprovação (reflete a decisão de produto: o desafio é público entre todo mundo que usa o app, não algo privado por grupo). Quem cria um desafio já entra automaticamente nele — decisão da IA, não pedida explicitamente, mas pareceu estranho um desafio sem o próprio criador participando.
- **Placar nunca é guardado** — é recalculado a cada vez que o desafio é aberto, contando os livros com `DataConclusao` dentro do período, exatamente como o `Status` ("Ativo"/"Encerrado") já era calculado e não guardado. Mesmo raciocínio, mesmo lugar do código que decide isso.
- **Streak é Duolingo-style**: conta dias consecutivos terminando hoje (se já marcou) ou ontem (se ainda não marcou hoje); um dia inteiro sem marcar quebra a sequência. Check-in é idempotente — marcar duas vezes no mesmo dia não duplica nada.
- **`Atividade` (o feed) tem dois níveis**: quatro tipos são gravados no momento em que acontecem (`TerminouLivro`, `ResenhaPublicada`, `MetaBatida`, `CheckIn` — ver `LivroService`/`StreakService`), e um quinto (`DesafioConcluido`) **nunca é gravado**, é calculado toda vez que alguém abre o feed, comparando a data de hoje com `DataFim` do desafio. O motivo: gravar esse evento exigiria rodar alguma rotina quando o desafio termina, e o projeto não tem (nem nunca teve) nenhum processo em segundo plano — tudo aqui é calculado sob demanda, na hora da requisição. Criar o primeiro "job" do projeto só pra isso pareceu desproporcional, e viola o padrão que todo o resto do código já segue.
- **Meta batida é detectada por cruzamento de limiar** (`antes < meta && depois >= meta`), não por um simples "antes/depois" ingênuo — evita registrar a mesma meta batida de novo a cada edição do livro, e evita contar errado quando um livro muda de ano de conclusão.
- **Exclusão de desafio** (`DELETE /api/desafios/{id}`) só pelo criador — endpoint que a IA acrescentou sem ter sido pedido, como forma de desfazer um desafio criado por engano; nada impede remover esse endpoint se não fizer sentido manter.
- Mesma regra de `OnDelete` que já existia para `Seguidor` (ver `EstanteDigitalDbContext`) foi repetida aqui (`Restrict` em `Desafio.CriadorId` e `ParticipacaoDesafio.UsuarioId`) — é o que evita o erro de "múltiplos caminhos de cascade" do SQL Server/Postgres quando duas FKs diferentes poderiam apagar a mesma linha em cascata.

## Docker

Três containers: banco (`postgres:16-alpine`, imagem oficial, sem Dockerfile próprio), backend e frontend — os dois últimos com Dockerfile multi-stage (compila numa imagem maior, roda numa imagem final enxuta). O frontend é servido por Nginx como arquivos estáticos — não existe um "servidor Node" rodando em produção. Detalhe que costuma pegar quem não fez isso antes: a variável `VITE_API_URL` é "queimada" dentro do JavaScript **no momento do build da imagem**, não em tempo de execução — por isso ela é um `ARG` do Dockerfile, e aponta para a porta que a API expõe no seu PC (quem faz a chamada é o navegador, não outro container).

A API só sobe depois que o Postgres responde de verdade a conexões (`depends_on: db: condition: service_healthy`, usando `pg_isready` como healthcheck) — sem isso, a primeira tentativa de conexão da API numa subida a frio poderia falhar por pura corrida entre os dois containers.

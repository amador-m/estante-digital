# Aula 20 — Projeto Final (Estante Digital)

Este é o projeto final do curso: não parte de nenhuma Aula anterior como "base" específica (a Aula 20 não cita uma, diferente das últimas), mas reaproveita blocos inteiros já testados nas Aulas 12/16 (JWT manual, refresh token, interceptors do Axios) — só que agora dentro de uma Clean Architecture de verdade, com Docker, e com um tema seu: uma estante de livros de fantasia/suspense internacional, em vez do catálogo de produtos genérico das aulas anteriores.

**Leia esta seção antes de postar no GitHub** — tem duas decisões que tomei no seu lugar e uma limitação de teste que você precisa saber.

## ⚠️ As decisões que tomei sem te perguntar (e por quê)

Você pediu "não pressuponha nada", então aqui vai exatamente onde presumi algo — pra você aprovar, corrigir ou pedir pra eu refazer:

1. **"Rafael Montes" → "Raphael Montes".** O autor de *Bom Dia, Verônica* e *Dias Perfeitos* se chama Raphael (com h) — corrigi a grafia.
2. **"L.C. Almeida" não existe em nenhuma bibliografia que eu consiga confirmar** como autora internacional de fantasia/suspense. Em vez de inventar uma sinopse para uma autora que pode nem existir (o que seria eu criando informação falsa), troquei por **Leigh Bardugo** (Grishaverse / Seis de Corvos) — também fantasia internacional, também best-seller, e que entra bem ao lado de Sarah J. Maas no catálogo. Se você tinha uma autora específica em mente com outro nome/grafia, me fala o nome certo que eu troco o catálogo (são só ~3 livros pra reescrever em `DadosIniciais.cs`).
3. ~~**Único usuário administrador** (não dois perfis como na Aula 16) — porque aqui o catálogo é público; só quem *gerencia* o acervo precisa de login, então não existia um segundo papel com propósito real.~~ **Superado na Fase 2** (ver seção abaixo): agora cada conta tem sua própria estante privada, então nem faz mais sentido ter um "admin" só — toda conta gerencia só a si mesma.
4. ~~**Persistência em arquivo JSON, não um banco de dados relacional** — porque o NuGet está bloqueado no meu ambiente.~~ **Também endereçado na Fase 2**: implementei a versão com PostgreSQL via EF Core (ela que roda de verdade no `docker compose up`), mantendo a versão em JSON só como caminho que eu consigo testar aqui — os detalhes de como isso foi feito, e o que continua sem eu ter testado, estão na seção "Fase 2" abaixo.
5. **README em português.** Dei a opção de inglês não ser necessária, já que o curso é em PT-BR — mas se o plano é usar isso como peça de portfólio pra vagas internacionais, é rápido eu te mandar uma versão em inglês também (ou só o README, mantendo o LEIA-ME em português pra você).

## ✅ Atualização: `docker compose up` foi testado de verdade (por você!)

Na primeira entrega, eu não tinha como testar o Docker no meu ambiente (o sandbox onde trabalho bloqueia o download de imagens-base, tanto do `mcr.microsoft.com` quanto do Docker Hub, com `403 Forbidden`) — só tinha validado a sintaxe e o padrão dos Dockerfiles. Você rodou `docker compose up --build` no seu PC e **funcionou de primeira**, sem nenhum erro: as duas imagens buildaram, os dois containers subiram, a API respondeu em `http://localhost:5230` e o front em `http://localhost:8080`. Essa lacuna está fechada — pode confiar que o `docker-compose.yml` funciona como está documentado no README.

## Evolução pós-entrega — "Fase 1" (tema claro/escuro e ranking)

Depois de confirmar que o Docker funcionava, você pediu melhorias pensando em portfólio, citando Skoob e GymRats como referência. Conversamos sobre prioridade e combinamos 4 fases — isto aqui é só a **Fase 1** (rápida, baixo risco, sem mudar arquitetura):

- **Alternância de tema claro/escuro.** Botão (☀️/🌙) na barra superior. Tecnicamente: o tema é decidido por um pequeno `<script>` no `index.html`, **antes** do React montar — é de propósito, pra evitar aquele flash de tela clara por uma fração de segundo toda vez que alguém que escolheu o escuro dá F5 (se isso fosse feito só com `useEffect` do React, o HTML já teria pintado claro antes do efeito rodar). A preferência fica em `localStorage`; na primeira visita (sem nada salvo), respeita o tema do sistema operacional da pessoa (`prefers-color-scheme`). Todas as cores do projeto já eram variáveis CSS desde a primeira entrega — isso que tornou a adição barata (bastou um segundo bloco de variáveis, não reescrever cada componente).
- **Ranking de livros** (inspirado no "melhores avaliados" do Skoob). Novo endpoint `GET /api/livros/ranking` (público, mesma regra de visibilidade do catálogo) — regra de negócio nova em `LivroService.ObterRankingAsync()`: só entram livros com status "Lido" **e** nota preenchida (um livro na fila não tem nota, não compete). Nova página `/ranking`, lista numerada com os 3 primeiros destacados. Reparem que essa é uma mudança **só na camada de Application + um Controller + uma página React** — nenhuma linha de `Domain` ou `Infrastructure` mudou, exatamente o tipo de evolução que a Clean Architecture existe para tornar barata.

A Fase 3 (estilo Skoob: resenha em texto, meta de leitura anual, progresso de leitura por página, seguir outras pessoas) já foi implementada — ver seção própria mais abaixo. A Fase 4 continua combinada pra depois (não implementada ainda, por decisão sua — é projeto de portfólio, não precisa estar pronto pra "lançar"):

- **Fase 4 (estilo GymRats):** desafios em grupo com prazo e placar, sequência de dias lendo (streak), feed de atividade.

## Evolução pós-entrega — Fase 2 (contas reais + PostgreSQL)

Esta foi a fase grande — bem maior que tema claro/escuro ou ranking, porque muda o **modelo de dados** do projeto inteiro, não só adiciona uma tela. Resumo das decisões, na ordem em que elas se encadeiam:

### 1. Modelo escolhido: "estante por usuário" (multi-tenant)

Você escolheu, quando te perguntei, o modelo onde **cada conta cadastrada tem sua própria estante privada** — não um catálogo único compartilhado com "quem pode editar" diferente de "quem só vê". Isso muda a pergunta de toda consulta de livro: antes era "quais livros existem", agora é sempre "quais livros **deste usuário** existem".

Pra isso não depender de ninguém lembrar de filtrar certo em cada lugar, segui o princípio de "nasce filtrado": `ILivroRepository.BuscarPorIdAsync(id, usuarioId)` recebe os dois juntos e só encontra o livro se as duas condições baterem — não existe um "acho por id e depois confiro se é do dono" em dois passos (onde alguém, no futuro, poderia esquecer o segundo passo). O mesmo vale pra listar, atualizar e remover.

Consequência direta: **o catálogo deixou de ser público.** Antes, qualquer visitante via a lista de livros sem login (fazia sentido com um único admin dono do acervo); agora, como toda estante é de alguém, todo o `LivrosController` e o `DashboardController` passaram a exigir `[Authorize]`, e o React inteiro (exceto `/login` e `/cadastro`) ficou atrás de `PrivateRoute`.

### 2. Conceito de "Role" (Admin) foi removido — não enfraquecido, removido

Na Aula 16 fazia sentido ter usuário comum × admin, porque a listagem também era protegida e havia uma hierarquia real. Aqui não existe mais essa hierarquia: toda conta gerencia **só a própria estante**, então não haveria nada de diferente pra um "Admin" fazer que um usuário comum não fizesse. Removi `Role` do token, do `TokenResponseDto`, do `BearerAuthenticationHandler` e troquei `[Authorize(Roles = "Admin")]` por `[Authorize]` simples — a claim que importa agora é `NameIdentifier` (o id do usuário), não um papel.

Removi também a classe `UsuarioSimulado.cs` inteira (não deixei comentada nem renomeada) — ela representava exatamente o modelo antigo (usuário fixo, senha comparada com `==`) que este refactor substitui; mantê-la seria código morto e, pior, um exemplo ruim esquecido no repositório.

### 3. Senha com hash de verdade (PBKDF2), sem depender de pacote nenhum

Esse era o problema de segurança mais sério da entrega anterior: a "senha" do admin fixo era comparada em texto puro (`usuario.Senha == senhaDigitada`). Com contas reais, isso vira inaceitável — qualquer vazamento do arquivo/banco expõe a senha de verdade de quem se cadastrou.

Troquei por PBKDF2-HMACSHA256 (RFC 2898) usando só `System.Security.Cryptography.Rfc2898DeriveBytes` — é literalmente o mesmo algoritmo que o `PasswordHasher` do ASP.NET Core Identity usa por baixo, só que escrito à mão (`Infrastructure/Auth/SenhaHasher.cs`), pela mesma razão de sempre neste projeto: nenhum pacote do NuGet pode ser baixado no ambiente onde eu trabalho, então ou eu escrevo com a biblioteca padrão do .NET, ou eu não consigo testar nada. 100.000 iterações, salt aleatório de 16 bytes por senha, comparação em tempo constante na verificação (mesmo cuidado que o `TokenService` já tomava com a assinatura do JWT).

### 4. Persistência real: PostgreSQL via EF Core — a parte que **não consegui testar**

Isto é o ponto mais importante desta seção, então vou ser bem direta.

Os pacotes `Npgsql.EntityFrameworkCore.PostgreSQL`/`Microsoft.EntityFrameworkCore` **não podem ser baixados no meu ambiente** — testei de novo agora (`dotnet add package`) só pra ter certeza de que a limitação continuava valendo antes de decidir como proceder, e continua: `403 Forbidden` do NuGet, mesmo bloqueio que já tinha impedido o Docker de puxar imagens na primeira entrega.

Isso me colocou diante de um problema maior do que da vez do Docker: lá, eu só não conseguia testar a *containerização* — toda a lógica de negócio continuava 100% testável rodando a API direto com `dotnet run`. Aqui, se eu simplesmente adicionasse os pacotes do EF Core ao projeto, **o projeto inteiro pararia de compilar no meu ambiente** — eu perderia a capacidade de verificar até a parte que não mudou (cadastro, login, isolamento por usuário), porque o build falharia na restauração dos pacotes antes mesmo de chegar no meu código.

A solução que escolhi: compilação condicional. O `EstanteDigital.Infrastructure.csproj` só referencia o pacote do Npgsql quando o build recebe a flag `-p:UsarPostgres=true`; sem ela (o padrão), os arquivos de `Persistencia/Ef/` (o `DbContext` e os dois repositórios EF Core) ficam de fora da compilação — protegidos por `#if USAR_POSTGRES` — e o projeto usa o repositório em arquivo JSON, exatamente como antes. Isso **não é um padrão geral que eu recomendaria** numa arquitetura normal (ter duas implementações de persistência é raro valer a pena fora desta situação específica) — é uma resposta direta à limitação do meu ambiente, documentada aqui com toda a honestidade, não uma "boa prática" genérica.

Na prática, isso significa:
- **Eu continuo com 100% de confiança** no que já era testável: toda a lógica de negócio de Fase 2 (cadastro, hash de senha, login, isolamento por conta, remoção de role) foi validada de ponta a ponta com Playwright, rodando a API localmente com o repositório JSON — os mesmos testes de sempre, cobrindo também os casos novos (duas contas diferentes não veem a estante uma da outra, cadastro com e-mail duplicado dá erro, etc.).
- **Eu não consegui compilar nem rodar nenhuma linha dentro de `Persistencia/Ef/`** (o `EstanteDigitalDbContext`, o `LivroRepositorioEfCore`, o `UsuarioRepositorioEfCore`) nem a configuração do Postgres no `docker-compose.yml`. Escrevi com o máximo de cuidado, seguindo o jeito padrão do EF Core (mesmas convenções que qualquer tutorial oficial usa), mas é código que só vai ser compilado e executado pela primeira vez **no seu `docker compose up --build`**, dentro da imagem do SDK, que tem acesso de verdade ao NuGet — exatamente como aconteceu com o Docker na primeira entrega, que você testou e funcionou de primeira.
- Escolhi `Database.EnsureCreated()` em vez de `Database.Migrate()` com uma pasta de `Migrations/` gerada por `dotnet ef migrations add` — por dois motivos: (a) gerar migrations exige rodar a ferramenta `dotnet-ef`, que eu também não conseguiria testar aqui; (b) para um projeto de portfólio que não precisa evoluir schema incrementalmente, `EnsureCreated` é mais simples e direto — cria as tabelas a partir do modelo atual na primeira subida. Se um dia este projeto crescer a ponto de precisar alterar o schema depois de já ter dados reais em produção, aí sim vale trocar para `Migrate()` com migrations de verdade.
- Semeei a conta demo (`demo@estantedigital.com` / `Estante@123`) com a mesma estante de 19 livros de antes, tanto na versão JSON quanto na versão Postgres — assim, seja qual for o jeito que alguém suba o projeto, não cai numa tela vazia. Uma conta nova, cadastrada de verdade, começa com a estante em branco (comportamento esperado e correto de um sistema multiusuário).

**O que eu peço pra você fazer:** rodar `docker compose up --build` (o comando não mudou) e confirmar que consegue logar com a conta demo, cadastrar uma conta nova, e que os livros persistem depois de um `docker compose down` + `up` de novo (sem `-v`). Se der algum erro de compilação ou de conexão com o Postgres, me manda a mensagem completa do log (`docker compose logs api`) que eu ajusto — é exatamente o tipo de ciclo que já funcionou bem da última vez.

### Testes que rodei na Fase 2

Com a API rodando localmente (`dotnet run`, repositório JSON — o único caminho que consigo executar aqui) e o front em `npm run dev`:

1. `dotnet build` (sem `-p:UsarPostgres`) → 0 erros, 0 warnings — confirma que nada do resto do projeto quebrou com o refactor.
2. `npm run build` (frontend) → build limpo.
3. Acessar `/` sem login → redireciona para `/login` (antes era público).
4. Criar conta nova pelo `/cadastro` → já loga automaticamente, cai em `/` com a estante **vazia** (conta nova, sem livros).
5. Logout → volta pra `/login`.
6. Login com a conta demo (`demo@estantedigital.com`) → mostra o catálogo de 19 livros de sempre.
7. Ranking e Dashboard carregam normalmente logado.
8. Página "Gerenciar" carrega com o texto explicativo atualizado (fala em autorização por dono, não mais em role).
9. Alternância de tema clara/escura (Fase 1) continua funcionando depois do refactor.
10. Via `curl` direto na API: duas contas diferentes criam livros e cada uma só vê os próprios (0 vazamento entre estantes); requisição sem token em `/api/livros` → `401`; cadastro com e-mail já existente → `400`; arquivo `usuarios.json` confirmado com senha armazenada como hash (`iterações.salt.hash`), nunca em texto puro.

## Evolução pós-entrega — Perfil, cor principal e nova logo

Você pediu três coisas numa tacada só: área de edição de perfil, uma cor principal configurável pra plataforma, e uma logo nova no lugar do emoji 📚. Perguntei o que exatamente deveria entrar no perfil, como a cor seria escolhida e onde ela seria salva — suas respostas definiram o design abaixo: **nome de usuário + senha + ícone** no perfil (não só "nome de exibição"), **paleta fixa de 8 cores prontas** (não um seletor de cor livre) e **persistência na conta, no banco** (não só `localStorage`).

### Por que paleta fixa e ícone fixo, não cor livre / upload de imagem

Decisão deliberada, não limitação técnica: com uma cor livre (`<input type=color>` ou hex digitado), qualquer combinação de contraste ruim (texto branco sobre amarelo claro, por exemplo) vira responsabilidade de validar; com upload de imagem como avatar, entra a necessidade de guardar arquivo em algum lugar (disco? S3? banco como blob?) e, de quebra, moderar o que é enviado. Nenhum dos dois problemas existe com um conjunto fechado de opções: as 8 cores (`Application/Constantes/OpcoesPerfil.cs`, espelhado em `frontend/src/constantes/perfil.js`) já têm variantes claro/escuro escolhidas à mão pra manter o mesmo nível de contraste da azul original, e os 8 ícones são SVG desenhados à mão (`IconePerfil.jsx`) — não tem "ícone enviado por engano" possível, porque não existe upload.

A mesma lógica resolveu o pedido da logo nova: criei `LogoEstante.jsx`, um livro aberto em SVG dentro de um quadrado com cantos arredondados, usando `var(--acento)` como cor de fundo — ou seja, a logo (e o emblema do ícone de perfil na barra) recolorem sozinhos junto com a cor que a conta escolheu. Isso não era estritamente pedido, mas amarra bem os dois pedidos: a logo "nova" e a "cor principal" viram a mesma coisa visualmente, em vez de duas features soltas uma do lado da outra.

### Nome de usuário: gerado sozinho no cadastro, trocável depois

A tela de cadastro continua pedindo só e-mail/senha (não adicionei um campo a mais ali) — o nome de usuário é derivado automaticamente da parte antes do `@` do e-mail (`AuthService.GerarNomeUsuarioUnicoAsync`), e se já existir alguém com esse nome, acrescenta um número (`maria`, `maria2`, `maria3`...) até achar um livre. A pessoa troca esse nome quando quiser em "Meu perfil". Único formato aceito: letras, números, ponto, traço e underline, 3 a 24 caracteres — validado nos dois lados (regex igual no `PerfilService.cs` e no formulário React).

### ⚠️ Se você já tem um volume do Postgres rodando desde a Fase 2, ele **não** vai ganhar as colunas novas sozinho

Isto é importante e é exatamente o tipo de limitação que avisei lá na Fase 2 que ia aparecer: a tabela `Usuarios` ganhou três colunas (`NomeUsuario`, `Icone`, `CorPrincipal`), mas a API continua usando `Database.EnsureCreated()` em vez de migrations de verdade — e `EnsureCreated()` **só cria tabelas que ainda não existem**; ele não compara o modelo com o banco e não altera uma tabela já criada. Ou seja: se seu Postgres já tem a tabela `Usuarios` de uma subida anterior (da Fase 2), ele vai continuar exatamente como estava, sem as 3 colunas novas — e a API provavelmente vai quebrar tentando ler/gravar um perfil.

**O que fazer:** antes de testar esta feature com Docker, rode `docker compose down -v` (o `-v` é o que de fato apaga o volume do Postgres) e depois `docker compose up --build` de novo, pra recriar o banco do zero já com o schema novo. Isso apaga os dados que estavam lá (inclusive contas de teste que você tenha criado) — se quiser manter dados reais no futuro, esse é o momento certo de trocar `EnsureCreated()` por migrations de verdade (`dotnet ef migrations add`), porque esse comando apagar-e-recriar deixa de ser uma opção aceitável assim que o projeto tem dados que importam. Pra este projeto de portfólio, por ora, `down -v` resolve.

### Bug pré-existente que encontrei (não era o que você pediu, mas corrigi)

Testando o formulário de perfil, encontrei um bug real no `AuthContext.jsx`, que já existia desde a Fase 2 e não tinha relação direta com o que você pediu agora — mas decidi corrigir porque é exatamente o tipo de coisa que "depois de implementar, verifique se funciona de verdade" deveria pegar: em modo de desenvolvimento (`npm run dev`), o `React.StrictMode` chama de propósito, duas vezes, o efeito que tenta renovar a sessão ao carregar a página — é um recurso do próprio React para expor efeitos colaterais mal escritos, não um bug do Vite. As duas chamadas usavam o mesmo refresh token (lido antes de qualquer uma delas terminar); dependendo da ordem em que as respostas chegavam, dava pra cair de volta na tela de login por um instante mesmo com uma sessão válida — ou, num caso mais sutil ainda, a chamada que falhava conseguia apagar do `localStorage` o token novo que a outra tinha acabado de salvar. Corrigi na raiz: agora uma trava (`useRef`) garante que o efeito de renovação só dispara a chamada de verdade uma vez, não importa quantas vezes o React invoque o efeito. Testei chamando essa renovação (reload completo da página, logo após logar) 5 vezes seguidas depois da correção — as 5 vezes ficaram limpas, uma única chamada de rede, sem cair na tela de login. **Isso só acontece em `npm run dev`; no build de produção (o que roda dentro do Docker) o StrictMode não dobra os efeitos, então esse bug nunca teria aparecido lá** — mas corrigir na origem é melhor do que confiar nisso.

### Testes que rodei nesta feature

Com a API rodando localmente (repositório JSON) e o front em `npm run dev`, Playwright cobrindo o fluxo completo:

1. Cadastro novo → nome de usuário derivado do e-mail aparece na barra superior automaticamente.
2. Entrar em "Meu perfil" → trocar nome de usuário, ícone e cor principal → mensagem de sucesso, barra superior reflete o nome novo, variável CSS `--acento` reflete a cor nova (lida via `getComputedStyle`, não só visualmente).
3. Tentar usar um nome de usuário que já existe (o da conta demo) → rejeitado com mensagem clara, formulário não fica em estado quebrado.
4. Trocar senha com a senha atual errada → rejeitado.
5. Trocar senha com a senha atual certa → sucesso.
6. Logout → login de novo com a senha nova → funciona.
7. F5 na página → nome de usuário e cor continuam os mesmos (prova que vêm do banco/backend, não só de estado local do React que se perderia num reload).
8. Regressão completa da suíte da Fase 2 (9 passos: redirecionamento sem login, cadastro, isolamento entre contas, logout, login da conta demo, ranking, dashboard, Gerenciar, tema) — todos continuam passando depois dessas mudanças.
9. `dotnet build` (sem `-p:UsarPostgres`) → 0 erros, 0 warnings. `npm run build` → build limpo (108 módulos, antes eram 102).

O que eu **não** testei, pela mesma limitação de sempre: o caminho com Postgres de verdade (via Docker) não pode ser compilado nem executado no meu ambiente — só testei a versão em arquivo JSON. Segue a mesma recomendação da Fase 2: depois de rodar `docker compose down -v && docker compose up --build`, confirme que dá pra entrar com a conta demo, editar o perfil (nome/ícone/cor) e que isso persiste depois de um `docker compose down` + `up` (sem `-v`, dessa vez).

## Evolução pós-entrega — Fase 3 (resenha, meta de leitura, progresso de página, seguir pessoas)

Esta é a feature "estilo Skoob" que ficou combinada lá atrás, na seção da Fase 1. Antes de implementar, te perguntei 3 coisas, porque cada uma mudava o design inteiro — suas respostas:

1. **Visibilidade da estante pra quem segue:** a estante inteira, read-only (não "só os lidos", nem campos escondidos).
2. **Meta de leitura:** simples, só o ano corrente — sem histórico ano a ano.
3. **Visibilidade da resenha:** visível pra quem te segue (junto com o resto da estante, não uma visibilidade separada).

A partir daí, tomei um bom número de decisões de design que você não especificou diretamente — seguindo o mesmo princípio de sempre ("não pressuponha nada" vira "avise onde você decidiu por conta própria"), aqui está cada uma:

### 1. Progresso de página e resenha: sem travar pelo status de leitura

`Livro.PaginaAtual` e `Livro.Resenha` não exigem que o status seja "Lendo" ou "Lido" pra existir — validei só o óbvio (página não-negativa, não maior que o número de páginas quando esse número existe; resenha até 4000 caracteres). Decidi não travar mais que isso porque trava gera atrito sem ganho real: por exemplo, alguém pode querer manter a página marcada mesmo depois de passar o livro pra "Lido", ou escrever uma resenha de um livro que já abandonou. Preferi confiar na pessoa que está preenchendo do que impor uma máquina de estados rígida que ninguém pediu.

### 2. `DataConclusao`: campo derivado, não editável — criei pra resolver a meta anual

Pra calcular "quantos livros você já leu este ano" (o número que a meta de leitura precisa), eu precisava de uma data de quando cada livro foi concluído — e isso **não existia** no modelo antes da Fase 3. Em vez de adicionar mais um campo pra pessoa preencher à mão (o que seria fácil de esquecer ou preencher errado), fiz `DataConclusao` ser **automática**: toda vez que o status de um livro muda **para** "Lido", a API carimba a data/hora atual; se o status sai de "Lido" pra qualquer outro, a data é apagada. Se o livro já estava "Lido" e só outro campo mudou (ex.: você corrigiu o título), a data de conclusão original é preservada, não é re-carimbada. Isso não foi pedido — é uma decisão minha pra fazer a meta anual funcionar sem exigir mais um input manual de você.

### 3. Visibilidade em duas camadas: estatísticas sempre visíveis, estante só pra quem segue

Você pediu "a estante inteira, read-only" pra quem segue — mas isso não diz o que uma pessoa vê **antes** de decidir seguir alguém. Decidi que **identidade e estatísticas** (nome de usuário, ícone, cor, data de criação da conta, meta anual, livros lidos no ano, total de seguidores/seguindo) ficam visíveis pra **qualquer conta logada** que acesse o perfil público de alguém — só a **estante completa** (lista de livros, com resenhas) fica atrás do "seguir". A ideia é a mesma de Instagram/Twitter: você consegue julgar se quer seguir alguém (quantos livros ela lê, que meta ela tem) sem já ter acesso ao conteúdo completo. Sem essa camada, a única forma de saber se vale a pena seguir alguém seria seguir primeiro e descobrir depois.

### 4. Seguir é imediato, sem pedido de aprovação (modelo Twitter/Instagram, não Facebook)

`POST /api/usuarios/{nomeUsuario}/seguir` já cria a relação na hora — não existe estado "pendente" nem a outra pessoa precisa aceitar. Escolhi esse modelo porque combina com a visibilidade do item 3: já que a estante de alguém só aparece de verdade depois de segui-la (não há "pedido" que a pessoa possa negar por privacidade), não fazia sentido adicionar uma etapa de aprovação que o Skoob/Goodreads, aliás, também não têm.

### 5. `PerfilSocialService` mora em `Application`, não em `Infrastructure` — não é inconsistência

Repare que ele não ficou ao lado do `AuthService`/`PerfilService` (que são Infrastructure). É proposital, e segue a mesma regra que já vale pro `LivroService`: um serviço só precisa estar em `Infrastructure` quando ele depende de um **detalhe técnico concreto** (o `AuthService` usa o `SenhaHasher` direto, por exemplo). O `PerfilSocialService` depende só de **interfaces** de repositório do `Domain` (`IUsuarioRepository`, `ISeguidorRepository`, `ILivroRepository`) — nenhuma dependência concreta —, então ele pertence em `Application`, exatamente como o `LivroService` já estava desde a entrega original.

### 6. Conta "amigaleitora": inventei pra dar pra testar "seguir" sem precisar criar uma segunda conta na mão

Pra esta feature fazer sentido, é preciso ter **pelo menos duas contas** — uma pra seguir a outra. Em vez de te pedir pra cadastrar uma segunda conta manualmente toda vez que quiser testar, semeei uma segunda conta de demonstração (`amiga@estantedigital.com` / `Estante@123`, nome de usuário `amigaleitora`, ícone de coruja, cor roxa, meta de 12 livros/ano) com uma estante própria de 4 livros (incluindo um "Lendo" com página marcada e dois "Lido" com resenha) — assim dá pra testar o fluxo de busca → ver perfil → seguir → ver estante liberada → resenha aparecendo, tudo de cara, sem precisar criar nada na mão primeiro. Essa conta é só pra demonstração, igual a `demo@estantedigital.com` já era.

### ⚠️ Mesmo aviso de sempre sobre `EnsureCreated()` — se seu Postgres já existe, rode `down -v`

A tabela `Usuarios` ganhou a coluna `MetaLeituraAnual` e existe uma tabela nova inteira (`Seguidores`) — e, como já expliquei na seção de Perfil acima, `EnsureCreated()` não altera uma tabela/banco que já existe, só cria o que ainda não existia. Se você já tinha rodado `docker compose up` antes desta fase, rode `docker compose down -v` (apaga o volume) e depois `docker compose up --build` de novo, senão a API vai quebrar tentando usar colunas/tabela que não existem no seu banco.

### Testes que rodei nesta feature

Com a API rodando localmente (repositório JSON) e o front em `npm run dev`, Playwright cobrindo o fluxo completo (10 passos, todos passando):

1. Login com a conta demo.
2. Editar um livro existente, preencher página atual + resenha, salvar, reabrir o formulário de edição e confirmar que os dois valores persistiram (a lista de "Gerenciar" não exibe esses campos, só o formulário de edição mostra).
3. Dashboard mostra a seção "Meta de leitura de 2026" com o texto "9 de 20 livros" (conta demo tem meta 20, 9 livros concluídos no ano).
4. Trocar a meta de leitura anual na página de Perfil → mensagem de sucesso.
5. Buscar "amiga" na página Pessoas → encontra "amigaleitora".
6. Visitar o perfil público dela **antes** de seguir → estatísticas aparecem, mas a estante fica escondida com a mensagem "Siga amigaleitora para ver a estante completa" (e o livro "O Nome do Vento" não aparece em lugar nenhum da página).
7. Clicar em "Seguir" → botão vira "Deixar de seguir", e a estante completa aparece, incluindo o livro "O Nome do Vento" e o texto da resenha dela ("Releitura. Continua sendo o melhor livro...").
8. Ela passa a aparecer em "Seguindo (1)" na página Pessoas.
9. Clicar em "Deixar de seguir" → estante volta a ficar escondida.
10. Visitar um nome de usuário que não existe (`/pessoas/naoexiste999`) → mostra "Usuário não encontrado." sem quebrar a página.

Regressão: as suítes da Fase 2 (9 passos) e da feature de Perfil (11 passos) continuam passando sem nenhuma mudança, rodadas de novo depois desta fase. `dotnet build EstanteDigital.sln` (sem `-p:UsarPostgres`) → 0 erros, 0 warnings em todos os 4 projetos. `npm run build` → build limpo.

O que eu **não** testei, pela mesma limitação de sempre: o caminho com Postgres de verdade (incluindo a tabela `Seguidores` nova e o índice único composto nela) não pode ser compilado nem executado no meu ambiente — só testei a versão em arquivo JSON. Mesma recomendação de sempre: depois do `docker compose down -v && docker compose up --build`, testar seguir/deixar de seguir e confirmar que persiste depois de um `down`/`up` normal (sem `-v`).

## O que foi construído

```
Aula20-entrega/
├── README.md                    ← a "cara" do repositório no GitHub
├── LEIA-ME_Aula20.md             ← este arquivo (contexto da aula/tutoria)
├── docker-compose.yml
├── docs/
│   ├── ARQUITETURA.md            ← explicação técnica da Clean Architecture
│   └── screenshot-catalogo.png
├── .github/workflows/ci.yml      ← build check (não é CI/CD completo — ver README)
└── src/
    ├── api/                      ← EstanteDigital.Api/.Domain/.Application/.Infrastructure
    └── frontend/                 ← React + Vite
```

### Backend — Clean Architecture de verdade (não só pastas)

Diferente das Aulas 11-16 (um único projeto, Controller chamando Repository direto), aqui existem **4 projetos .csproj separados** com referências de um sentido só: `Api → Infrastructure → Application → Domain`. Cada camada e o porquê da divisão estão explicados com mais detalhe (e um diagrama) em `docs/ARQUITETURA.md` — vale a pena ler antes de uma entrevista/apresentação, porque é exatamente o tipo de pergunta que o PDF desta aula avisa que vai cair ("capacidade de explicar decisões técnicas").

Resumo rápido de onde fica cada critério do "Code Review Profissional" que o PDF lista:

| Critério do PDF | Onde está |
|---|---|
| Clean Architecture aplicada | 4 projetos com dependência num sentido só (`Domain` não depende de nada) |
| Controllers enxutos | `Controllers/*.cs` — só recebem DTO, chamam o Service, devolvem resultado |
| Services com regras | `LivroService.cs` — validação de título/autor/ano/sinopse, regra "só avalia quem foi Lido", filtros, agregação do dashboard |
| DTOs corretos | `Application/DTOs/` — nunca expõe a entidade `Livro` direto na API |
| Logs estruturados | *não implementado* — ver "o que ficou de fora" abaixo |
| Docker funcional | `docker-compose.yml` + Dockerfile de cada projeto (ver aviso acima sobre teste) |
| README claro | `README.md` na raiz |

### O que ficou de fora de propósito (seja direta sobre isso numa apresentação)

- **Logs estruturados** (Serilog, etc.) — o PDF lista no checklist de code review, mas não há nenhuma instrução de como implementar, e adicionar isso bem (com correlação de requisição, por exemplo) merece atenção própria em vez de ser plugado às pressas. Fica como próximo passo real, não só enfeite de README.
- **Testes automatizados no próprio repositório** (xUnit/Vitest) — eu testei tudo com Playwright *fora* do repositório (no meu ambiente), mas não deixei esses testes dentro do projeto porque não fazem parte do que o PDF pediu como entrega desta aula especificamente (ele cita xUnit/Jest em "Próximos Passos Recomendados", ou seja, também é evolução futura). Se quiser, eu formalizo isso como suíte de testes dentro do repo depois.

### Frontend

React + Vite + React Router, tema claro com tons de azul (`#2563eb` como cor de destaque), telas: Catálogo (público, com busca e filtros), Detalhe do livro, Dashboard (público, estatísticas), Login, e Gerenciar acervo (autenticado — CRUD completo). Reaproveita a mesma arquitetura de autenticação da Aula 16 (token em memória, refresh token em `localStorage`, interceptors do Axios renovando sozinho em um 401, `PrivateRoute`) — sem repetir aqui o que já está documentado lá; se quiser revisitar esses detalhes, o `LEIA-ME_Aula16.md` da entrega anterior continua valendo.

## Testes que rodei na Fase 1 (tema + ranking)

Build limpo de novo depois das mudanças (`dotnet build` e `npm run build`, 0 erros), e um novo teste Playwright cobrindo especificamente o que mudou, mais regressão do que já existia:

1. Catálogo continua carregando normalmente (regressão)
2. Tema inicial é claro por padrão (sem preferência salva)
3. Clicar no botão alterna pra escuro (confirmado lendo a variável CSS `--bg-pagina` calculada, não só visualmente)
4. Tema escuro persiste depois de recarregar a página (confirma que o `localStorage` + o script anti-flash do `index.html` funcionam juntos)
5. Página `/ranking` carrega com os livros avaliados, ordenados por nota
6. Login/admin continuam funcionando (regressão)

## Testes que rodei na entrega original (fora do Docker, como expliquei acima)

Build:
- `dotnet build EstanteDigital.sln` → 0 erros, 0 warnings
- `npm run build` (frontend) → build limpo
- `npx oxlint src/` → só os mesmos avisos benignos de sempre (`set-state-in-effect` em efeitos de carregamento inicial, `only-export-components` no Context) — nenhum erro

Funcional (Playwright, API + front rodando via `dotnet run`/`npm run dev`), 13 passos, todos passando:

1. Catálogo público carrega sem login (19 livros)
2. Filtro por gênero funciona (10 de Fantasia)
3. Busca textual funciona ("corvos" → 1 resultado)
4. Página de detalhe de um livro abre
5. Dashboard público carrega com o total correto
6. `/admin` sem login redireciona para `/login`
7. Login inválido mostra erro
8. Login válido entra em `/admin`
9. Criar livro pelo formulário funciona
10. Editar livro funciona
11. Excluir livro funciona (acervo volta ao estado original)
12. Logout volta para `/login`
13. F5 em `/admin` mantém a sessão (renovação silenciosa do token, igual Aula 16)

## Como rodar no seu PC (passo a passo, pra postar no GitHub)

### Pré-requisitos
- **Opção A (Docker):** Docker Desktop instalado e aberto.
- **Opção B (sem Docker):** .NET 8 SDK + Node.js 20+ instalados.

### Opção A — Docker (um comando só)

```bash
git clone <seu-repositorio>
cd <pasta-do-repositorio>
docker compose up --build
```

Agora sobem **três** containers (banco Postgres + API + frontend, antes eram só dois) — espera a mensagem de que todos subiram, depois abre `http://localhost:8080` no navegador. Logue com a conta demo (`demo@estantedigital.com` / `Estante@123`, já populada com os 19 livros) ou cadastre uma conta nova pelo link "Cadastrar". Pra parar: `Ctrl+C` e depois `docker compose down` (os dados do Postgres ficam guardados num volume entre execuções — só somem se você rodar `docker compose down -v`).

### Opção B — Modo desenvolvimento (sem Docker, sem Postgres)

```bash
# Terminal 1 — API
cd src/api/EstanteDigital.Api
dotnet run
# sobe em http://localhost:5230

# Terminal 2 — Frontend
cd src/frontend
npm install
npm run dev
# sobe em http://localhost:5173 — acesse este endereço no navegador
```

Sem a flag `-p:UsarPostgres=true` (que só o Dockerfile usa), a API sobe com o repositório em arquivo JSON — útil pra desenvolver/testar rápido sem precisar do Postgres rodando. Os dados ficam em `src/api/EstanteDigital.Api/bin/Debug/net8.0/Dados/` e a conta demo é a mesma (`demo@estantedigital.com` / `Estante@123`).

### Se algo der errado

- **Porta 5230, 8080 ou 5432 já em uso:** troque a porta publicada no `docker-compose.yml` (lado esquerdo do `:`) ou feche o programa que já está usando a porta.
- **Frontend sobe mas não carrega livros (erro de rede no console do navegador):** confirme que a API subiu antes (no modo Docker, às vezes o frontend fica pronto alguns segundos antes da API — só atualizar a página depois de ~10s resolve, já que não há orquestração de "espera a API responder" configurada por simplicidade).
- **No modo Docker, erro de CORS:** confirme que acessou `http://localhost:8080` e não outra porta — o backend só aceita requisições vindas exatamente das origens listadas em `appsettings.json` (`Cors:OrigensPermitidas`).
- **No modo Docker, erro ao carregar/salvar o perfil, a meta de leitura ou "seguir" (ou a API nem sobe) e você já tinha rodado este projeto antes da feature de perfil ou da Fase 3:** rode `docker compose down -v` (apaga o volume do Postgres) e depois `docker compose up --build` de novo — ver "Evolução pós-entrega — Perfil, cor principal e nova logo" e "Evolução pós-entrega — Fase 3" acima, seção do `EnsureCreated()`, pra entender por quê.
- **No modo Docker, API não conecta no Postgres:** o `docker-compose.yml` já faz a API esperar o banco responder (`depends_on: db: condition: service_healthy`) antes de subir — se mesmo assim der erro de conexão, roda `docker compose logs db` pra ver se o Postgres realmente terminou de inicializar, e `docker compose logs api` pra ver a mensagem de erro completa. Essa é a parte que eu não consegui testar no meu ambiente (ver LEIA-ME, seção Fase 2) — me manda o log se travar que eu ajusto.

## Prints

- `docs/screenshot-catalogo.png` / `docs/screenshot-catalogo-escuro.png` — catálogo, tema claro e escuro.
- `docs/screenshot-perfil.png` — página de "Meu perfil" (nome de usuário, ícone, cor principal).
- `docs/screenshot-perfil-publico.png` — perfil público de outra conta depois de segui-la: estatísticas, estante liberada com barra de progresso e resenha.
- Mais telas (dashboard com meta de leitura, formulário de edição, busca de pessoas) foram usadas só para validar o funcionamento durante os testes e não foram incluídas no repositório pra não inflar o README — me avisa se quiser que eu adicione mais alguma ao `docs/`.

# 📚 Estante Digital

Sistema full stack de gestão de acervo literário — catálogo de livros, dashboard de leitura e área administrativa autenticada, com Clean Architecture no backend e deploy via Docker.

> Projeto final do curso de ASP.NET Core + React (Aula 20 — Projeto Final e Apresentação), com tema próprio: uma estante pessoal de livros de fantasia e suspense internacional (Sarah J. Maas, Raphael Montes, Leigh Bardugo, Mariana Zapata).

| Tema claro | Tema escuro |
|---|---|
| ![Catálogo — tema claro](docs/screenshot-catalogo.png) | ![Catálogo — tema escuro](docs/screenshot-catalogo-escuro.png) |

![Página de perfil — nome de usuário, ícone e cor principal](docs/screenshot-perfil.png)

![Perfil público de outra conta, depois de seguir — estante liberada com progresso de leitura e resenha](docs/screenshot-perfil-publico.png)

## 📌 Descrição

A Estante Digital é um catálogo de livros com:

- **Conta própria por usuário** — cada pessoa cadastrada tem sua própria estante privada; ninguém vê ou edita os livros de outra conta.
- **Dashboard e ranking pessoais** — estatísticas (total de livros, lidos/lendo/na fila, nota média, distribuição por gênero e por autor) e um ranking dos livros melhor avaliados (inspirado nas listas do Skoob), ambos calculados sobre a estante de quem está logado.
- **Tema claro/escuro** — alternável a qualquer momento, com a preferência lembrada entre visitas.
- **Cadastro e login com senha de verdade** — hash PBKDF2 (nunca texto puro), autenticação por JWT com refresh token.
- **Perfil personalizável** — nome de usuário (gerado a partir do e-mail no cadastro, trocável depois), ícone e cor de destaque da interface, escolhidos entre opções fixas; troca de senha.
- **Resenha, progresso de leitura e meta anual** — resenha em texto por livro, página atual (com barra de progresso), e uma meta de livros lidos no ano corrente, acompanhada no dashboard.
- **Seguir outras pessoas** — busque por nome de usuário e siga outras contas para ver a estante completa delas (livros, status e resenhas); antes de seguir, só as estatísticas públicas do perfil ficam visíveis.

## 🛠️ Stack

- **Backend:** ASP.NET Core 8 (Minimal hosting + Controllers), autenticação JWT manual (sem pacotes externos de terceiros)
- **Frontend:** React 19 + Vite, React Router, Axios com interceptors
- **Persistência:** PostgreSQL via EF Core (produção/Docker) — ver *Decisões técnicas* no `LEIA-ME_Aula20.md` para o porquê de também existir um caminho alternativo em arquivo JSON
- **Infraestrutura:** Docker / Docker Compose (API + frontend + banco Postgres), Nginx (serving do build do React)

## 🧱 Arquitetura

Clean Architecture no backend, em 4 projetos:

```
src/api/
├── EstanteDigital.Domain          (entidades, enums, interfaces de repositório — sem depender de nada)
├── EstanteDigital.Application     (DTOs, serviços com as regras de negócio, interfaces de serviço)
├── EstanteDigital.Infrastructure  (JWT, hash de senha, repositórios — PostgreSQL/EF Core e JSON)
└── EstanteDigital.Api             (Controllers, Program.cs — composição de tudo)
```

Detalhes de cada camada e o porquê da divisão: [`docs/ARQUITETURA.md`](docs/ARQUITETURA.md).

## 🚀 Execução

### Opção A — Docker (recomendado para avaliar o projeto rápido)

```bash
docker compose up --build
```

- Frontend: http://localhost:8080
- API: http://localhost:5230 (health check em `/health`)
- Banco: PostgreSQL, container próprio (`db`), dados num volume Docker

### Opção B — Modo desenvolvimento (dois terminais, sem Postgres)

```bash
# Terminal 1
cd src/api/EstanteDigital.Api
dotnet run

# Terminal 2
cd src/frontend
npm install
npm run dev
```

- Frontend: http://localhost:5173
- API: http://localhost:5230 (persistência em arquivo JSON nesse modo — ver `LEIA-ME_Aula20.md`)

Passo a passo completo, com solução de problemas comuns: [`LEIA-ME_Aula20.md`](LEIA-ME_Aula20.md).

### Conta de demonstração

| E-mail                        | Senha         |
|-------------------------------|---------------|
| `demo@estantedigital.com`     | `Estante@123` |

Já vem com uma estante de 19 livros cadastrados. Ou crie sua própria conta pelo link "Cadastrar" na tela de login — cada conta começa com a estante vazia.

> Projeto de portfólio/estudo — credencial de demonstração fixa no código de propósito, visível no repositório. Não reutilize essa senha em nenhum projeto real.

## 🔐 Segurança

- Senhas com hash PBKDF2-HMACSHA256 (100.000 iterações, salt aleatório por conta) — nunca texto puro
- Autenticação JWT (assinatura HMAC-SHA256, implementação manual, sem `Microsoft.AspNetCore.Authentication.JwtBearer`)
- Refresh token com rotação, revogado no logout
- Toda rota de `/api/livros` e `/api/dashboard` exige `[Authorize]` e filtra pelo id de quem está logado — uma conta nunca enxerga nem edita livros de outra
- CORS restrito às origens do próprio frontend (dev e Docker)

## 📈 Próximos passos

- Desafios de leitura em grupo e feed de atividade, inspirados no GymRats
- Testes automatizados (xUnit no backend, Vitest no frontend)
- Migrations de verdade via `dotnet-ef` (hoje o schema do Postgres é criado com `EnsureCreated`, suficiente para este estágio do projeto — ver `LEIA-ME_Aula20.md`)
- CI/CD completo (o workflow atual em `.github/workflows/ci.yml` só valida o build, não faz deploy)
- Observabilidade (OpenTelemetry) e deploy em nuvem (Azure/AWS)

## 📖 Sobre o catálogo inicial

Os livros e autores do catálogo de demonstração são reais (títulos, séries e anos conferidos); as sinopses são redações originais sobre a premissa pública de cada livro. Status de leitura e notas são dados fictícios, só para dar conteúdo ao dashboard — edite à vontade pela área administrativa depois de logar.

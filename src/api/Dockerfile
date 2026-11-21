# Multi-stage build: compila com o SDK completo, mas a imagem final só
# leva o runtime ASP.NET — bem menor (relevante pra quem for rodar isso
# fora do notebook do curso, tipo um free tier de cloud).

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia só os .csproj primeiro — assim o "dotnet restore" fica em cache do
# Docker e só roda de novo quando alguma dependência muda, não a cada
# alteração de código.
COPY EstanteDigital.Domain/EstanteDigital.Domain.csproj EstanteDigital.Domain/
COPY EstanteDigital.Application/EstanteDigital.Application.csproj EstanteDigital.Application/
COPY EstanteDigital.Infrastructure/EstanteDigital.Infrastructure.csproj EstanteDigital.Infrastructure/
COPY EstanteDigital.Api/EstanteDigital.Api.csproj EstanteDigital.Api/

# -p:UsarPostgres=true liga a persistência real (PostgreSQL via EF Core) —
# por padrão (sem essa flag) o projeto usa o repositório em arquivo JSON,
# que é o que eu consigo compilar e testar no meu ambiente de trabalho (ver
# EstanteDigital.Infrastructure.csproj e LEIA-ME_Aula20.md, seção Fase 2,
# para o porquê). Aqui, dentro da imagem do SDK, com acesso de verdade ao
# NuGet, é a primeira vez que esse caminho é efetivamente compilado.
RUN dotnet restore EstanteDigital.Api/EstanteDigital.Api.csproj -p:UsarPostgres=true

COPY . .
RUN dotnet publish EstanteDigital.Api/EstanteDigital.Api.csproj -c Release -o /app --no-restore -p:UsarPostgres=true

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app .

# A API escuta na 8080 dentro do container (padrão da imagem aspnet:8.0);
# o docker-compose.yml é quem decide pra qual porta do seu PC isso é
# exposto (5230, pra bater com o .env do front sem precisar mudar nada).
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["dotnet", "EstanteDigital.Api.dll"]

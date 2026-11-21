# Build do React com Node, depois serve os arquivos estáticos com Nginx —
# a imagem final não carrega Node nem node_modules, só HTML/CSS/JS prontos.

FROM node:22-alpine AS build
WORKDIR /app

COPY package*.json ./
RUN npm install

COPY . .

# O Vite "queima" variáveis VITE_* dentro do JavaScript NO MOMENTO DO BUILD
# (não em tempo de execução, como seria num backend). Por isso aqui é um
# ARG/build-arg, não uma variável de ambiente do container em runtime —
# definir isso só no "environment:" do docker-compose não teria efeito
# nenhum no código já compilado. O valor por padrão aponta para a porta
# que a API expõe NO SEU PC (5230, via docker-compose), porque quem faz a
# chamada é o navegador rodando na sua máquina, não um container de dentro
# da rede do Docker — "http://api:8080" (o nome do serviço) só funcionaria
# se fosse outro container chamando a API, não o navegador.
ARG VITE_API_URL=http://localhost:5230/api
ENV VITE_API_URL=$VITE_API_URL
RUN npm run build

FROM nginx:1.27-alpine AS final
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 8080

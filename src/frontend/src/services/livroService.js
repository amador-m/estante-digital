import api from "./api";

export function listarLivros(filtros = {}) {
  const parametros = {};
  if (filtros.genero) parametros.genero = filtros.genero;
  if (filtros.autor) parametros.autor = filtros.autor;
  if (filtros.status) parametros.status = filtros.status;
  if (filtros.busca) parametros.busca = filtros.busca;

  return api.get("/livros", { params: parametros }).then((r) => r.data);
}

export function buscarLivroPorId(id) {
  return api.get(`/livros/${id}`).then((r) => r.data);
}

export function criarLivro(dados) {
  return api.post("/livros", dados).then((r) => r.data);
}

export function atualizarLivro(id, dados) {
  return api.put(`/livros/${id}`, dados).then((r) => r.data);
}

export function excluirLivro(id) {
  return api.delete(`/livros/${id}`);
}

export function obterDashboard() {
  return api.get("/dashboard").then((r) => r.data);
}

export function obterRanking() {
  return api.get("/livros/ranking").then((r) => r.data);
}

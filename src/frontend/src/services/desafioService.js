import api from "./api";

export function listarDesafios() {
  return api.get("/desafios").then((r) => r.data);
}

export function listarMeusDesafios() {
  return api.get("/desafios/meus").then((r) => r.data);
}

export function obterDesafio(id) {
  return api.get(`/desafios/${id}`).then((r) => r.data);
}

export function criarDesafio(dados) {
  return api.post("/desafios", dados).then((r) => r.data);
}

export function entrarNoDesafio(id) {
  return api.post(`/desafios/${id}/entrar`);
}

export function sairDoDesafio(id) {
  return api.delete(`/desafios/${id}/entrar`);
}

export function excluirDesafio(id) {
  return api.delete(`/desafios/${id}`);
}

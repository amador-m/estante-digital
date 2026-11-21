import api from "./api";

export function buscarUsuarios(trecho) {
  return api.get("/usuarios/buscar", { params: { q: trecho } }).then((r) => r.data);
}

export function obterPerfilPublico(nomeUsuario) {
  return api.get(`/usuarios/${encodeURIComponent(nomeUsuario)}/perfil-publico`).then((r) => r.data);
}

export function seguir(nomeUsuario) {
  return api.post(`/usuarios/${encodeURIComponent(nomeUsuario)}/seguir`);
}

export function deixarDeSeguir(nomeUsuario) {
  return api.delete(`/usuarios/${encodeURIComponent(nomeUsuario)}/seguir`);
}

export function listarSeguindo() {
  return api.get("/usuarios/me/seguindo").then((r) => r.data);
}

export function listarSeguidores() {
  return api.get("/usuarios/me/seguidores").then((r) => r.data);
}

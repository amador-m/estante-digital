import api from "./api";

export function obterPerfil() {
  return api.get("/perfil").then((r) => r.data);
}

export function atualizarPerfil(dados) {
  return api.put("/perfil", dados).then((r) => r.data);
}

export function alterarSenha(dados) {
  return api.put("/perfil/senha", dados);
}

import axios from "axios";

// Cliente separado do `api.js` principal: login/refresh/logout não devem
// passar pelos interceptors de token (evita um ciclo — "preciso de token
// pra renovar o token").
const authApi = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
});

export function registrar(email, senha) {
  return authApi.post("/auth/registro", { email, senha });
}

export function login(email, senha) {
  return authApi.post("/auth/login", { email, senha });
}

export function renovar(refreshToken) {
  return authApi.post("/auth/refresh", { refreshToken });
}

export function logoutNaApi(refreshToken) {
  return authApi.post("/auth/logout", { refreshToken });
}

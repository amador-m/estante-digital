import axios from "axios";
import { getAuthToken, setAuthToken, clearAuthToken, CHAVE_REFRESH_TOKEN } from "./tokenStore";
import { renovar } from "./authService";

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
});

// Anexa o Bearer token em toda requisição autenticada (as rotas de leitura
// do catálogo funcionam normalmente sem isso — só os endpoints de
// escrita exigem).
api.interceptors.request.use((config) => {
  const token = getAuthToken();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Evita disparar várias renovações em paralelo se várias requisições
// tomarem 401 ao mesmo tempo — todas esperam a mesma promise.
let renovacaoEmAndamento = null;

function irParaLogin() {
  clearAuthToken();
  localStorage.removeItem(CHAVE_REFRESH_TOKEN);
  window.location.href = "/login";
}

api.interceptors.response.use(
  (resposta) => resposta,
  async (erro) => {
    const { config, response } = erro;

    if (response?.status !== 401 || config._jaTentouRenovar) {
      return Promise.reject(erro);
    }

    const refreshToken = localStorage.getItem(CHAVE_REFRESH_TOKEN);
    if (!refreshToken) {
      irParaLogin();
      return Promise.reject(erro);
    }

    config._jaTentouRenovar = true;

    try {
      if (!renovacaoEmAndamento) {
        renovacaoEmAndamento = renovar(refreshToken).finally(() => {
          renovacaoEmAndamento = null;
        });
      }

      const { data } = await renovacaoEmAndamento;
      setAuthToken(data.accessToken);
      localStorage.setItem(CHAVE_REFRESH_TOKEN, data.refreshToken);

      config.headers.Authorization = `Bearer ${data.accessToken}`;
      return api.request(config);
    } catch {
      irParaLogin();
      return Promise.reject(erro);
    }
  }
);

export default api;

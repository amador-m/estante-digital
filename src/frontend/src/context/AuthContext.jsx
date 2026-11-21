import { createContext, useCallback, useEffect, useRef, useState } from "react";
import { login as loginNaApi, registrar as registrarNaApi, renovar, logoutNaApi } from "../services/authService";
import { obterPerfil } from "../services/perfilService";
import { setAuthToken, clearAuthToken, CHAVE_REFRESH_TOKEN } from "../services/tokenStore";
import { decodificarPayloadJwt } from "../services/jwt";

export const AuthContext = createContext(null);

function montarUsuario(accessToken) {
  const payload = decodificarPayloadJwt(accessToken);
  return {
    email: payload?.email ?? null,
  };
}

export function AuthProvider({ children }) {
  const [token, setToken] = useState(null);
  const [usuario, setUsuario] = useState(null);
  // Perfil completo (nome de usuário, ícone, cor) não vem no token — é
  // carregado à parte, de forma assíncrona, logo após autenticar. Enquanto
  // não chega, a interface usa o e-mail do token como texto provisório.
  const [perfil, setPerfil] = useState(null);
  // Enquanto `carregando` for true, o PrivateRoute espera em vez de já
  // mandar pro /login — dá tempo da tentativa de renovação silenciosa abaixo
  // terminar (é o que persiste a sessão depois de um F5).
  const [carregando, setCarregando] = useState(true);

  // Guarda contra o efeito de renovação abaixo rodar duas vezes: em
  // desenvolvimento (npm run dev), o React.StrictMode invoca efeitos de
  // montagem duas vezes de propósito, só para expor efeitos colaterais mal
  // escritos. Sem essa guarda, as duas chamadas disparavam duas requisições
  // de renovação com o MESMO refresh token; dependendo de qual resposta
  // chegava primeiro, a pessoa podia acabar vendo a tela de login por um
  // instante mesmo com uma sessão válida (a renovação que falhava podia
  // zerar o estado antes da que tinha sucesso terminar). Isso nunca
  // acontece em produção (`npm run build`), onde o StrictMode não dobra os
  // efeitos — mas protegemos aqui mesmo assim, pra não depender disso.
  const efeitoDeRenovacaoJaRodou = useRef(false);

  const carregarPerfil = useCallback(() => {
    obterPerfil()
      .then(setPerfil)
      .catch(() => {
        // Se isso falhar (rede instável, etc.), o resto da interface
        // continua funcionando com o e-mail vindo do token.
      });
  }, []);

  useEffect(() => {
    if (efeitoDeRenovacaoJaRodou.current) {
      return;
    }
    efeitoDeRenovacaoJaRodou.current = true;

    const refreshTokenSalvo = localStorage.getItem(CHAVE_REFRESH_TOKEN);

    if (!refreshTokenSalvo) {
      setCarregando(false);
      return;
    }

    renovar(refreshTokenSalvo)
      .then(({ data }) => {
        setAuthToken(data.accessToken);
        localStorage.setItem(CHAVE_REFRESH_TOKEN, data.refreshToken);
        setToken(data.accessToken);
        setUsuario(montarUsuario(data.accessToken));
        carregarPerfil();
      })
      .catch(() => {
        // Refresh token ausente/expirado/inválido de verdade (não é mais o
        // caso do StrictMode, já bloqueado acima) — desloga de fato.
        localStorage.removeItem(CHAVE_REFRESH_TOKEN);
      })
      .finally(() => setCarregando(false));
  }, [carregarPerfil]);

  async function login(email, senha) {
    const { data } = await loginNaApi(email, senha);
    setAuthToken(data.accessToken);
    localStorage.setItem(CHAVE_REFRESH_TOKEN, data.refreshToken);
    setToken(data.accessToken);
    setUsuario(montarUsuario(data.accessToken));
    carregarPerfil();
  }

  async function registrar(email, senha) {
    const { data } = await registrarNaApi(email, senha);
    // Cadastro já devolve os tokens — a pessoa não precisa logar de novo.
    setAuthToken(data.accessToken);
    localStorage.setItem(CHAVE_REFRESH_TOKEN, data.refreshToken);
    setToken(data.accessToken);
    setUsuario(montarUsuario(data.accessToken));
    carregarPerfil();
  }

  async function logout() {
    const refreshTokenSalvo = localStorage.getItem(CHAVE_REFRESH_TOKEN);
    if (refreshTokenSalvo) {
      // Revoga no servidor antes de limpar localmente — sem isso, o refresh
      // token continuaria válido por 7 dias mesmo após o "logout".
      try {
        await logoutNaApi(refreshTokenSalvo);
      } catch {
        // Mesmo se a API estiver fora do ar, ainda limpamos a sessão local.
      }
    }

    clearAuthToken();
    localStorage.removeItem(CHAVE_REFRESH_TOKEN);
    setToken(null);
    setUsuario(null);
    setPerfil(null);
  }

  return (
    <AuthContext.Provider
      value={{ token, usuario, perfil, setPerfil, carregando, login, registrar, logout, isAuth: !!token }}
    >
      {children}
    </AuthContext.Provider>
  );
}

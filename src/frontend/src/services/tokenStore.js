// Access token só em memória (nunca em localStorage/sessionStorage) — se a
// aba recarregar, ele some, e é por isso que existe a renovação silenciosa
// no AuthContext (usando o refresh token, esse sim persistido).
let token = null;

export function setAuthToken(t) {
  token = t;
}

export function getAuthToken() {
  return token;
}

export function clearAuthToken() {
  token = null;
}

export const CHAVE_REFRESH_TOKEN = "estante_refreshToken";

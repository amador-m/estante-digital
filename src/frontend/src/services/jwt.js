// Decodifica (NÃO valida) o payload de um JWT. Serve só para exibir o
// e-mail na interface — qualquer um consegue ler isso sem a chave
// secreta, então nunca é usado para decidir o que a pessoa pode fazer.
// Quem valida de verdade é sempre a API, a cada requisição.
export function decodificarPayloadJwt(token) {
  try {
    const partes = token.split(".");
    if (partes.length !== 3) return null;

    const base64 = partes[1].replace(/-/g, "+").replace(/_/g, "/");
    const texto = decodeURIComponent(
      atob(base64)
        .split("")
        .map((c) => "%" + c.charCodeAt(0).toString(16).padStart(2, "0"))
        .join("")
    );
    return JSON.parse(texto);
  } catch {
    return null;
  }
}

import { useEffect, useState } from "react";

const CHAVE_TEMA = "estante-tema";

/**
 * O tema inicial já foi aplicado na tag <html> por um script no index.html
 * (antes do React montar, pra evitar o "flash" de tema errado). Este hook
 * só sincroniza o estado do React com o que já está no DOM, e persiste a
 * escolha quando a pessoa clica no botão.
 */
export function useTemaEscuro() {
  const [temaEscuro, setTemaEscuro] = useState(
    () => document.documentElement.getAttribute("data-tema") === "escuro"
  );

  useEffect(() => {
    document.documentElement.setAttribute("data-tema", temaEscuro ? "escuro" : "claro");
    try {
      localStorage.setItem(CHAVE_TEMA, temaEscuro ? "escuro" : "claro");
    } catch {
      // localStorage indisponível (modo privado, etc.) — o tema ainda
      // funciona nesta sessão, só não é lembrado na próxima visita.
    }
  }, [temaEscuro]);

  return [temaEscuro, () => setTemaEscuro((atual) => !atual)];
}

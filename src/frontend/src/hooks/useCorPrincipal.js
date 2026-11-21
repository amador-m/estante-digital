import { useEffect } from "react";
import { obterPaleta } from "../constantes/perfil";

/**
 * Aplica a cor principal escolhida pela conta como variáveis CSS no <html>,
 * sobrescrevendo em tempo real os valores estáticos definidos no
 * index.css (que continuam servindo de "azul padrão" pra quem ainda não
 * logou ou cujo perfil ainda não carregou — sem flash feio, só o azul
 * de sempre até a troca).
 *
 * Reage tanto à cor quanto ao tema (claro/escuro) porque cada cor tem uma
 * variante pra cada tema — ver PALETA_CORES.
 */
export function useCorPrincipal(corPrincipal, temaEscuro) {
  useEffect(() => {
    const paleta = obterPaleta(corPrincipal);
    const variante = temaEscuro ? paleta.escuro : paleta.claro;
    const raiz = document.documentElement.style;

    raiz.setProperty("--acento", variante.acento);
    raiz.setProperty("--acento-escuro", variante.acentoEscuro);
    raiz.setProperty("--acento-claro", variante.acentoClaro);
    raiz.setProperty("--acento-texto", variante.acentoTexto);
  }, [corPrincipal, temaEscuro]);
}

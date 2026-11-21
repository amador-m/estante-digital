// Marca própria da Estante Digital — substitui o emoji 📚 usado antes. É um
// livro aberto desenhado em linhas simples sobre um selo colorido; o selo
// usa var(--acento), então a logo acompanha a cor principal escolhida por
// quem está logado (ver useCorPrincipal) em vez de ser uma cor fixa.
export default function LogoEstante({ tamanho = 28 }) {
  return (
    <svg
      width={tamanho}
      height={tamanho}
      viewBox="0 0 40 40"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
      aria-hidden="true"
    >
      <rect width="40" height="40" rx="11" fill="var(--acento)" />
      <path
        d="M9 13.5c0-1 1.3-1.6 3.4-1.6 2.6 0 5 1 7.6 2.6 2.6-1.6 5-2.6 7.6-2.6 2.1 0 3.4.6 3.4 1.6v14c0 .9-1.3 1.3-3.4 1.3-2.6 0-5-.9-7.6-2.4-2.6 1.5-5 2.4-7.6 2.4-2.1 0-3.4-.4-3.4-1.3Z"
        stroke="white"
        strokeWidth="1.8"
        strokeLinejoin="round"
        strokeLinecap="round"
      />
      <path d="M20 14.5V27" stroke="white" strokeWidth="1.8" strokeLinecap="round" />
    </svg>
  );
}

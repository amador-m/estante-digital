// Ícones de perfil: um conjunto fixo (ver constantes/perfil.js e o
// comentário em OpcoesPerfil.cs sobre por que não é upload de imagem
// livre), desenhados à mão em SVG simples — sem depender de nenhuma
// biblioteca de ícones externa. O badge usa var(--acento) como fundo, igual
// à logo, então também acompanha a cor principal da conta.

const TRACO = { stroke: "white", strokeWidth: 1.7, strokeLinecap: "round", strokeLinejoin: "round", fill: "none" };

function GlifoLivroAberto() {
  return (
    <>
      <path d="M12 10.5c0-.7.9-1.1 2.4-1.1 1.9 0 3.6.7 5.6 1.9 2-1.2 3.7-1.9 5.6-1.9 1.5 0 2.4.4 2.4 1.1v10c0 .6-.9.9-2.4.9-1.9 0-3.6-.6-5.6-1.8-2 1.2-3.7 1.8-5.6 1.8-1.5 0-2.4-.3-2.4-.9Z" {...TRACO} />
      <path d="M20 11v10.4" {...TRACO} />
    </>
  );
}

function GlifoPilhaLivros() {
  return (
    <>
      <rect x="11" y="20" width="18" height="3.4" rx="1" {...TRACO} />
      <rect x="12.5" y="15.6" width="15" height="3.4" rx="1" {...TRACO} />
      <rect x="10.5" y="11.2" width="13" height="3.4" rx="1" {...TRACO} />
    </>
  );
}

function GlifoMarcaPagina() {
  return <path d="M14 9h12v18l-6-4.3L14 27Z" {...TRACO} />;
}

function GlifoCoruja() {
  return (
    <>
      <path d="M20 10c-5 0-8 3.8-8 8.5S15.4 27 20 27s8-3.9 8-8.5S25 10 20 10Z" {...TRACO} />
      <circle cx="16.3" cy="17.8" r="2.1" {...TRACO} />
      <circle cx="23.7" cy="17.8" r="2.1" {...TRACO} />
      <path d="M20 19.5 18.7 22h2.6Z" {...TRACO} />
      <path d="M13 12.5 16 10M27 12.5 24 10" {...TRACO} />
    </>
  );
}

function GlifoPena() {
  return (
    <>
      <path d="M27 11c-7 .6-11.2 5-12.3 12.6 7.6-1.1 12-5.4 12.6-12.3.1-.3-.1-.4-.3-.3Z" {...TRACO} />
      <path d="M14.5 25.5 22 18" {...TRACO} />
    </>
  );
}

function GlifoOculos() {
  return (
    <>
      <circle cx="15" cy="19" r="4" {...TRACO} />
      <circle cx="25" cy="19" r="4" {...TRACO} />
      <path d="M19 19h2M11 19l-2-1M29 19l2-1" {...TRACO} />
    </>
  );
}

function GlifoXicara() {
  return (
    <>
      <path d="M12 16h12v6a6 6 0 0 1-6 6 6 6 0 0 1-6-6Z" {...TRACO} />
      <path d="M24 17.5h2a2.8 2.8 0 0 1 0 5.6h-2" {...TRACO} />
      <path d="M15 11c0 1 1 1 1 2s-1 1-1 2M19.5 11c0 1 1 1 1 2s-1 1-1 2" {...TRACO} />
    </>
  );
}

function GlifoEstrela() {
  return <path d="M20 10.5 22.6 16l6 .9-4.3 4.2 1 6-5.3-2.8-5.3 2.8 1-6-4.3-4.2 6-.9Z" {...TRACO} strokeLinejoin="round" />;
}

const GLIFOS = {
  "livro-aberto": GlifoLivroAberto,
  "pilha-livros": GlifoPilhaLivros,
  "marca-pagina": GlifoMarcaPagina,
  coruja: GlifoCoruja,
  pena: GlifoPena,
  oculos: GlifoOculos,
  xicara: GlifoXicara,
  estrela: GlifoEstrela,
};

// corFundo: normalmente deixado de fora — o badge usa var(--acento), a cor
// da PRÓPRIA conta que está logada. Só é passado explicitamente num lugar:
// o perfil público de outra conta (ver PerfilPublico.jsx), pra mostrar o
// ícone na cor que AQUELA pessoa escolheu, sem recolorir o resto da tela
// (que continua na cor de quem está vendo).
export default function IconePerfil({ icone, tamanho = 28, comFundo = true, corFundo }) {
  const Glifo = GLIFOS[icone] ?? GlifoLivroAberto;

  return (
    <svg width={tamanho} height={tamanho} viewBox="0 0 40 40" xmlns="http://www.w3.org/2000/svg" aria-hidden="true">
      {comFundo && <rect width="40" height="40" rx="20" fill={corFundo ?? "var(--acento)"} />}
      <Glifo />
    </svg>
  );
}

// Mesmas chaves (e só estas) que EstanteDigital.Application/Constantes/OpcoesPerfil.cs
// aceita e valida no backend. Se mudar uma lista, mudar a outra também —
// decisão consciente de não ter um endpoint só pra sincronizar isso (ver
// comentário no arquivo C# correspondente).

export const COR_PADRAO = "azul";
export const ICONE_PADRAO = "livro-aberto";

// Cada cor tem uma variante para o tema claro e outra pro escuro — mesmo
// papel que --acento/--acento-escuro/--acento-claro/--acento-texto já
// tinham fixos no index.css, só que agora parametrizados por conta.
export const PALETA_CORES = [
  {
    valor: "azul",
    rotulo: "Azul",
    claro: { acento: "#2563eb", acentoEscuro: "#1d4ed8", acentoClaro: "#dbe9fd", acentoTexto: "#ffffff" },
    escuro: { acento: "#3b82f6", acentoEscuro: "#60a5fa", acentoClaro: "#1e2a42", acentoTexto: "#ffffff" },
  },
  {
    valor: "roxo",
    rotulo: "Roxo",
    claro: { acento: "#7c3aed", acentoEscuro: "#6d28d9", acentoClaro: "#ede9fe", acentoTexto: "#ffffff" },
    escuro: { acento: "#8b5cf6", acentoEscuro: "#a78bfa", acentoClaro: "#2e1f4d", acentoTexto: "#ffffff" },
  },
  {
    valor: "rosa",
    rotulo: "Rosa",
    claro: { acento: "#db2777", acentoEscuro: "#be185d", acentoClaro: "#fce7f3", acentoTexto: "#ffffff" },
    escuro: { acento: "#ec4899", acentoEscuro: "#f472b6", acentoClaro: "#4a1d35", acentoTexto: "#ffffff" },
  },
  {
    valor: "vermelho",
    rotulo: "Vermelho",
    claro: { acento: "#dc2626", acentoEscuro: "#b91c1c", acentoClaro: "#fee2e2", acentoTexto: "#ffffff" },
    escuro: { acento: "#ef4444", acentoEscuro: "#f87171", acentoClaro: "#4a1515", acentoTexto: "#ffffff" },
  },
  {
    valor: "laranja",
    rotulo: "Laranja",
    claro: { acento: "#ea580c", acentoEscuro: "#c2410c", acentoClaro: "#ffedd5", acentoTexto: "#ffffff" },
    escuro: { acento: "#f97316", acentoEscuro: "#fb923c", acentoClaro: "#4a2410", acentoTexto: "#ffffff" },
  },
  {
    valor: "verde",
    rotulo: "Verde",
    claro: { acento: "#16a34a", acentoEscuro: "#15803d", acentoClaro: "#dcfce7", acentoTexto: "#ffffff" },
    escuro: { acento: "#22c55e", acentoEscuro: "#4ade80", acentoClaro: "#123023", acentoTexto: "#ffffff" },
  },
  {
    valor: "teal",
    rotulo: "Teal",
    claro: { acento: "#0d9488", acentoEscuro: "#0f766e", acentoClaro: "#ccfbf1", acentoTexto: "#ffffff" },
    escuro: { acento: "#2dd4bf", acentoEscuro: "#5eead4", acentoClaro: "#0f2e2b", acentoTexto: "#ffffff" },
  },
  {
    valor: "grafite",
    rotulo: "Grafite",
    claro: { acento: "#475569", acentoEscuro: "#334155", acentoClaro: "#e2e8f0", acentoTexto: "#ffffff" },
    escuro: { acento: "#94a3b8", acentoEscuro: "#cbd5e1", acentoClaro: "#1e293b", acentoTexto: "#0b1120" },
  },
];

export const ICONES_PERFIL = [
  { valor: "livro-aberto", rotulo: "Livro aberto" },
  { valor: "pilha-livros", rotulo: "Pilha de livros" },
  { valor: "marca-pagina", rotulo: "Marcador" },
  { valor: "coruja", rotulo: "Coruja" },
  { valor: "pena", rotulo: "Pena" },
  { valor: "oculos", rotulo: "Óculos" },
  { valor: "xicara", rotulo: "Xícara" },
  { valor: "estrela", rotulo: "Estrela" },
];

export function obterPaleta(valor) {
  return PALETA_CORES.find((p) => p.valor === valor) ?? PALETA_CORES.find((p) => p.valor === COR_PADRAO);
}

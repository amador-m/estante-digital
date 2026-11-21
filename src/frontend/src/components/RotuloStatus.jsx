const ROTULOS = {
  QueroLer: { texto: "Quero ler", classe: "status-querer-ler" },
  Lendo: { texto: "Lendo", classe: "status-lendo" },
  Lido: { texto: "Lido", classe: "status-lido" },
};

export default function RotuloStatus({ status }) {
  const info = ROTULOS[status] ?? { texto: status, classe: "" };
  return <span className={`rotulo-status ${info.classe}`}>{info.texto}</span>;
}

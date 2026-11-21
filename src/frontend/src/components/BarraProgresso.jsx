/// Barra de progresso de leitura — só faz sentido mostrar a barra em si
/// quando dá pra calcular uma porcentagem (numeroPaginas preenchido); sem
/// isso, mostra só o texto "página X" (a pessoa ainda assim queria
/// registrar onde parou, mesmo sem saber o total de páginas da edição).
export default function BarraProgresso({ paginaAtual, numeroPaginas }) {
  if (paginaAtual == null) {
    return null;
  }

  if (!numeroPaginas) {
    return <p className="progresso-leitura-texto">Página {paginaAtual}</p>;
  }

  const percentual = Math.min(100, Math.round((paginaAtual / numeroPaginas) * 100));

  return (
    <div className="progresso-leitura">
      <div className="progresso-leitura-barra">
        <div className="progresso-leitura-preenchido" style={{ width: `${percentual}%` }} />
      </div>
      <span className="progresso-leitura-texto">
        Página {paginaAtual} de {numeroPaginas} ({percentual}%)
      </span>
    </div>
  );
}

import { Link } from "react-router-dom";
import BarraProgresso from "./BarraProgresso";
import Estrelas from "./Estrelas";
import RotuloStatus from "./RotuloStatus";

export default function CartaoLivro({ livro }) {
  return (
    <Link to={`/livros/${livro.id}`} className="cartao-livro">
      <div className="cartao-livro-topo">
        <span className="cartao-genero">{livro.genero}</span>
        <RotuloStatus status={livro.statusLeitura} />
      </div>

      <h3 className="cartao-titulo">{livro.titulo}</h3>
      <p className="cartao-autor">{livro.autor}</p>
      {livro.serie && <p className="cartao-serie">Série: {livro.serie}</p>}

      <p className="cartao-sinopse">{livro.sinopseCurta}</p>

      {livro.statusLeitura === "Lendo" && (
        <BarraProgresso paginaAtual={livro.paginaAtual} numeroPaginas={livro.numeroPaginas} />
      )}

      <div className="cartao-rodape">
        <span className="cartao-ano">{livro.anoPublicacao}</span>
        <Estrelas nota={livro.avaliacao} />
      </div>
    </Link>
  );
}

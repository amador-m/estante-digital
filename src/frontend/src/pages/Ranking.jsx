import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { obterRanking } from "../services/livroService";
import Estrelas from "../components/Estrelas";

export default function Ranking() {
  const [livros, setLivros] = useState(null);
  const [erro, setErro] = useState(null);

  useEffect(() => {
    obterRanking()
      .then(setLivros)
      .catch(() => setErro("Não foi possível carregar o ranking. Verifique se a API está rodando."));
  }, []);

  if (erro) return <div className="erro-api">{erro}</div>;
  if (!livros) return <div className="carregando">Carregando</div>;

  return (
    <div className="pagina-ranking">
      <h1>Ranking</h1>
      <p className="subtitulo">
        Os livros melhor avaliados da estante — só entram livros marcados como "Lido" e com nota.
      </p>

      {livros.length === 0 ? (
        <div className="lista-vazia">Nenhum livro avaliado ainda. Marque um livro como "Lido" e dê uma nota na área administrativa.</div>
      ) : (
        <ol className="ranking-lista">
          {livros.map((livro, indice) => (
            <li key={livro.id} className={`ranking-item ${indice < 3 ? "ranking-top3" : ""}`}>
              <span className="ranking-posicao">#{indice + 1}</span>
              <Link to={`/livros/${livro.id}`} className="ranking-info">
                <p className="ranking-titulo">{livro.titulo}</p>
                <p className="ranking-autor">{livro.autor}</p>
              </Link>
              <span className="ranking-estrelas">
                <Estrelas nota={livro.avaliacao} />
              </span>
            </li>
          ))}
        </ol>
      )}
    </div>
  );
}

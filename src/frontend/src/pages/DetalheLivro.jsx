import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import BarraProgresso from "../components/BarraProgresso";
import Estrelas from "../components/Estrelas";
import RotuloStatus from "../components/RotuloStatus";
import { buscarLivroPorId } from "../services/livroService";

export default function DetalheLivro() {
  const { id } = useParams();
  const [livro, setLivro] = useState(null);
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState(null);

  useEffect(() => {
    setCarregando(true);
    buscarLivroPorId(id)
      .then(setLivro)
      .catch(() => setErro("Livro não encontrado."))
      .finally(() => setCarregando(false));
  }, [id]);

  if (carregando) return <div className="carregando">Carregando</div>;
  if (erro || !livro) return <div className="erro-api">{erro ?? "Livro não encontrado."}</div>;

  return (
    <div className="pagina-detalhe">
      <Link to="/" className="link-voltar">
        ← Voltar ao catálogo
      </Link>

      <div className="detalhe-cartao">
        <div className="detalhe-cabecalho">
          <span className="cartao-genero">{livro.genero}</span>
          <RotuloStatus status={livro.statusLeitura} />
        </div>

        <h1>{livro.titulo}</h1>
        <p className="detalhe-autor">{livro.autor}</p>
        {livro.serie && <p className="detalhe-serie">Série: {livro.serie}</p>}

        <div className="detalhe-meta">
          <span>Publicado em {livro.anoPublicacao}</span>
          {livro.numeroPaginas && <span>{livro.numeroPaginas} páginas</span>}
        </div>

        {livro.avaliacao && (
          <div className="detalhe-avaliacao">
            <Estrelas nota={livro.avaliacao} />
          </div>
        )}

        {livro.paginaAtual != null && (
          <div className="detalhe-progresso">
            <BarraProgresso paginaAtual={livro.paginaAtual} numeroPaginas={livro.numeroPaginas} />
          </div>
        )}

        <p className="detalhe-sinopse">{livro.sinopseCurta}</p>

        {livro.resenha && (
          <div className="detalhe-resenha">
            <h2>Resenha</h2>
            <p>{livro.resenha}</p>
          </div>
        )}
      </div>
    </div>
  );
}

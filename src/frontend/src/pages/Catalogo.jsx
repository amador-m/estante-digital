import { useEffect, useState } from "react";
import BarraDeFiltros from "../components/BarraDeFiltros";
import CartaoLivro from "../components/CartaoLivro";
import { listarLivros } from "../services/livroService";

export default function Catalogo() {
  const [livros, setLivros] = useState([]);
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState(null);
  const [filtros, setFiltros] = useState({ genero: "", status: "", busca: "" });

  useEffect(() => {
    let cancelado = false;
    setCarregando(true);
    setErro(null);

    // Pequeno debounce pra não disparar uma requisição a cada tecla digitada na busca.
    const timeout = setTimeout(() => {
      listarLivros(filtros)
        .then((dados) => {
          if (!cancelado) setLivros(dados);
        })
        .catch(() => {
          if (!cancelado) setErro("Não foi possível carregar o catálogo. Verifique se a API está rodando.");
        })
        .finally(() => {
          if (!cancelado) setCarregando(false);
        });
    }, 300);

    return () => {
      cancelado = true;
      clearTimeout(timeout);
    };
  }, [filtros]);

  return (
    <div className="pagina-catalogo">
      <div className="catalogo-cabecalho">
        <div>
          <h1>Minha Estante</h1>
          <p className="subtitulo">
            {carregando ? "Carregando..." : `${livros.length} livro${livros.length === 1 ? "" : "s"} no catálogo`}
          </p>
        </div>
      </div>

      <BarraDeFiltros filtros={filtros} aoMudar={setFiltros} />

      {erro && <div className="erro-api">{erro}</div>}

      {!erro && !carregando && livros.length === 0 && (
        <div className="lista-vazia">Nenhum livro encontrado com esses filtros.</div>
      )}

      <div className="grade-livros">
        {livros.map((livro) => (
          <CartaoLivro key={livro.id} livro={livro} />
        ))}
      </div>
    </div>
  );
}

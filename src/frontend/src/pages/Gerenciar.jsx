import { useEffect, useState } from "react";
import FormularioLivro from "../components/FormularioLivro";
import RotuloStatus from "../components/RotuloStatus";
import Estrelas from "../components/Estrelas";
import { atualizarLivro, criarLivro, excluirLivro, listarLivros } from "../services/livroService";

export default function Gerenciar() {
  const [livros, setLivros] = useState([]);
  const [carregando, setCarregando] = useState(true);
  const [erroLista, setErroLista] = useState(null);

  const [livroEmEdicao, setLivroEmEdicao] = useState(null);
  const [enviandoForm, setEnviandoForm] = useState(false);
  const [erroForm, setErroForm] = useState(null);

  const [idExcluindo, setIdExcluindo] = useState(null);

  function recarregar() {
    setCarregando(true);
    listarLivros()
      .then(setLivros)
      .catch(() => setErroLista("Não foi possível carregar o catálogo."))
      .finally(() => setCarregando(false));
  }

  useEffect(recarregar, []);

  async function salvar(dados) {
    setEnviandoForm(true);
    setErroForm(null);

    try {
      if (livroEmEdicao) {
        await atualizarLivro(livroEmEdicao.id, dados);
      } else {
        await criarLivro(dados);
      }
      setLivroEmEdicao(null);
      recarregar();
    } catch (erro) {
      setErroForm(erro.response?.data?.mensagem ?? "Não foi possível salvar o livro.");
    } finally {
      setEnviandoForm(false);
    }
  }

  async function excluir(id) {
    setIdExcluindo(id);
    try {
      await excluirLivro(id);
      recarregar();
    } finally {
      setIdExcluindo(null);
    }
  }

  return (
    <div className="pagina-admin">
      <h1>Gerenciar minha estante</h1>
      <p className="admin-explicacao">
        Esta área exige login porque todas as rotas de livros no backend têm <code>[Authorize]</code> e filtram
        pelo id de quem está logado — o que te protege aqui não é um papel especial de "admin": é que cada conta
        só acessa (e só consegue editar ou excluir) os livros que ela mesma cadastrou, mesmo que alguém chame a
        API diretamente, sem passar pelo React.
      </p>

      <FormularioLivro
        livroEmEdicao={livroEmEdicao}
        aoSalvar={salvar}
        aoCancelar={() => {
          setLivroEmEdicao(null);
          setErroForm(null);
        }}
        enviando={enviandoForm}
        erro={erroForm}
      />

      <h2 className="admin-lista-titulo">Acervo atual</h2>

      {erroLista && <div className="erro-api">{erroLista}</div>}
      {carregando && <div className="carregando">Carregando</div>}

      <div className="admin-lista">
        {livros.map((livro) => (
          <div className="admin-item" key={livro.id}>
            <div className="admin-item-info">
              <div className="admin-item-titulo-linha">
                <strong>{livro.titulo}</strong>
                <RotuloStatus status={livro.statusLeitura} />
              </div>
              <span className="admin-item-autor">
                {livro.autor} · {livro.genero} · {livro.anoPublicacao}
              </span>
              <Estrelas nota={livro.avaliacao} />
            </div>

            <div className="admin-item-acoes">
              <button
                className="btn-editar"
                onClick={() => {
                  setLivroEmEdicao(livro);
                  setErroForm(null);
                  window.scrollTo({ top: 0, behavior: "smooth" });
                }}
              >
                Editar
              </button>
              <button
                className="btn-remover"
                disabled={idExcluindo === livro.id}
                onClick={() => excluir(livro.id)}
              >
                {idExcluindo === livro.id ? "Removendo..." : "Excluir"}
              </button>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

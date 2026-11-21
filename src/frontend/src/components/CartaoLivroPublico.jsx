import BarraProgresso from "./BarraProgresso";
import Estrelas from "./Estrelas";
import RotuloStatus from "./RotuloStatus";

// Versão somente-leitura de CartaoLivro, usada na estante pública de quem
// você segue (PerfilPublico.jsx). Não é um link para /livros/:id de
// propósito: aquela rota busca o livro já filtrado pelo id de QUEM ESTÁ
// LOGADO (ver LivrosController), então abriria "não encontrado" para um
// livro de outra conta — mostrar os dados aqui mesmo, sem navegar, evita
// esse beco sem saída. Também mostra a resenha, que o cartão comum (da
// própria estante) não mostra, porque é justamente o que dá sentido a
// seguir alguém.
export default function CartaoLivroPublico({ livro }) {
  return (
    <div className="cartao-livro cartao-livro-publico">
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

      {livro.resenha && <p className="cartao-resenha">“{livro.resenha}”</p>}

      <div className="cartao-rodape">
        <span className="cartao-ano">{livro.anoPublicacao}</span>
        <Estrelas nota={livro.avaliacao} />
      </div>
    </div>
  );
}

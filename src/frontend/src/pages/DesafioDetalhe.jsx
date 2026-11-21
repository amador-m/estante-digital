import { useEffect, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import IconePerfil from "../components/IconePerfil";
import { obterPaleta } from "../constantes/perfil";
import { useTemaEscuro } from "../hooks/useTemaEscuro";
import { entrarNoDesafio, excluirDesafio, obterDesafio, sairDoDesafio } from "../services/desafioService";

export default function DesafioDetalhe() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [temaEscuro] = useTemaEscuro();

  const [detalhe, setDetalhe] = useState(null);
  const [erro, setErro] = useState(null);
  const [processando, setProcessando] = useState(false);

  function carregar() {
    obterDesafio(id)
      .then(setDetalhe)
      .catch(() => setErro("Desafio não encontrado."));
  }

  useEffect(carregar, [id]);

  function alternarParticipacao() {
    setProcessando(true);
    const acao = detalhe.desafio.voceParticipa ? sairDoDesafio(id) : entrarNoDesafio(id);
    acao.then(carregar).finally(() => setProcessando(false));
  }

  function excluir() {
    setProcessando(true);
    excluirDesafio(id).then(() => navigate("/desafios"));
  }

  if (erro) return <div className="erro-api">{erro}</div>;
  if (!detalhe) return <div className="carregando">Carregando</div>;

  const { desafio, placar } = detalhe;

  return (
    <div className="pagina-desafio-detalhe">
      <Link to="/desafios" className="link-voltar">
        ← Voltar
      </Link>

      <div className="cartao-desafio-detalhe-cabecalho">
        <div>
          <h1>{desafio.titulo}</h1>
          {desafio.descricao && <p className="perfil-dica">{desafio.descricao}</p>}
          <span className="cartao-desafio-meta">
            {new Date(desafio.dataInicio).toLocaleDateString("pt-BR")} –{" "}
            {new Date(desafio.dataFim).toLocaleDateString("pt-BR")} ·{" "}
            <span className={`rotulo-status-desafio ${desafio.status === "Ativo" ? "ativo" : "encerrado"}`}>
              {desafio.status}
            </span>
          </span>
        </div>

        <div className="cartao-desafio-detalhe-acoes">
          {desafio.status === "Ativo" && (
            <button
              type="button"
              className={desafio.voceParticipa ? "btn-cancelar" : "btn-participar"}
              onClick={alternarParticipacao}
              disabled={processando}
            >
              {desafio.voceParticipa ? "Sair do desafio" : "Entrar no desafio"}
            </button>
          )}
          {desafio.voceCriou && (
            <button type="button" className="btn-remover" onClick={excluir} disabled={processando}>
              Excluir desafio
            </button>
          )}
        </div>
      </div>

      <section>
        <h2>Placar — livros lidos no período</h2>
        {placar.length === 0 ? (
          <p className="perfil-dica">Ainda não há participantes neste desafio.</p>
        ) : (
          <div className="placar-desafio">
            {placar.map((p, i) => (
              <div key={p.usuarioId} className="placar-linha">
                <span className="placar-posicao">{i + 1}º</span>
                <IconePerfil
                  icone={p.icone}
                  tamanho={32}
                  corFundo={obterPaleta(p.corPrincipal)[temaEscuro ? "escuro" : "claro"].acento}
                />
                <span className="placar-nome">{p.nomeUsuario}</span>
                <span className="placar-contagem">
                  {p.livrosLidosNoPeriodo} {p.livrosLidosNoPeriodo === 1 ? "livro" : "livros"}
                </span>
              </div>
            ))}
          </div>
        )}
      </section>
    </div>
  );
}

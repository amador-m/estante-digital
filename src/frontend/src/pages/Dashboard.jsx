import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { obterDashboard } from "../services/livroService";
import { obterStreak, registrarCheckIn } from "../services/streakService";

function BarraContagem({ chave, quantidade, maximo }) {
  const largura = maximo > 0 ? Math.max(6, Math.round((quantidade / maximo) * 100)) : 0;
  return (
    <div className="barra-contagem-linha">
      <span className="barra-contagem-rotulo">{chave}</span>
      <div className="barra-contagem-trilho">
        <div className="barra-contagem-preenchimento" style={{ width: `${largura}%` }} />
      </div>
      <span className="barra-contagem-valor">{quantidade}</span>
    </div>
  );
}

export default function Dashboard() {
  const [dados, setDados] = useState(null);
  const [erro, setErro] = useState(null);

  const [streak, setStreak] = useState(null);
  const [marcandoCheckIn, setMarcandoCheckIn] = useState(false);

  useEffect(() => {
    obterDashboard()
      .then(setDados)
      .catch(() => setErro("Não foi possível carregar o dashboard. Verifique se a API está rodando."));
    obterStreak().then(setStreak);
  }, []);

  function marcarCheckIn() {
    setMarcandoCheckIn(true);
    registrarCheckIn()
      .then(setStreak)
      .finally(() => setMarcandoCheckIn(false));
  }

  if (erro) return <div className="erro-api">{erro}</div>;
  if (!dados) return <div className="carregando">Carregando</div>;

  const maximoGenero = Math.max(...dados.porGenero.map((g) => g.quantidade), 1);
  const maximoAutor = Math.max(...dados.porAutor.map((a) => a.quantidade), 1);

  return (
    <div className="pagina-dashboard">
      <h1>Dashboard</h1>

      <div className="cartoes-estatisticas">
        <div className="cartao-estatistica">
          <span className="estatistica-numero">{dados.totalDeLivros}</span>
          <span className="estatistica-rotulo">livros no total</span>
        </div>
        <div className="cartao-estatistica destaque-lido">
          <span className="estatistica-numero">{dados.totalLidos}</span>
          <span className="estatistica-rotulo">lidos</span>
        </div>
        <div className="cartao-estatistica destaque-lendo">
          <span className="estatistica-numero">{dados.totalLendo}</span>
          <span className="estatistica-rotulo">lendo agora</span>
        </div>
        <div className="cartao-estatistica destaque-querer-ler">
          <span className="estatistica-numero">{dados.totalQueroLer}</span>
          <span className="estatistica-rotulo">na fila</span>
        </div>
        <div className="cartao-estatistica">
          <span className="estatistica-numero">{dados.mediaAvaliacaoLidos ?? "—"}</span>
          <span className="estatistica-rotulo">nota média dos lidos</span>
        </div>
      </div>

      {streak && (
        <section className="dashboard-streak">
          <div>
            <span className="streak-numero">{streak.sequenciaAtual}</span>
            <span className="streak-rotulo">
              {streak.sequenciaAtual === 1 ? "dia seguido lendo" : "dias seguidos lendo"}
            </span>
          </div>
          <button type="button" onClick={marcarCheckIn} disabled={streak.jaMarcouHoje || marcandoCheckIn}>
            {streak.jaMarcouHoje ? "Já marcado hoje" : marcandoCheckIn ? "Marcando..." : "Marquei que li hoje"}
          </button>
        </section>
      )}

      {dados.metaLeituraAnual != null && (
        <section className="dashboard-meta-anual">
          <h2>Meta de leitura de {new Date().getFullYear()}</h2>
          <div className="progresso-leitura">
            <div className="progresso-leitura-barra">
              <div
                className="progresso-leitura-preenchido"
                style={{
                  width: `${Math.min(100, Math.round((dados.livrosLidosNoAnoAtual / dados.metaLeituraAnual) * 100))}%`,
                }}
              />
            </div>
            <span className="progresso-leitura-texto">
              {dados.livrosLidosNoAnoAtual} de {dados.metaLeituraAnual} livros
              {dados.livrosLidosNoAnoAtual >= dados.metaLeituraAnual ? " — meta batida!" : ""}
            </span>
          </div>
        </section>
      )}

      <div className="dashboard-colunas">
        <section className="dashboard-bloco">
          <h2>Livros por gênero</h2>
          {dados.porGenero.map((g) => (
            <BarraContagem key={g.chave} chave={g.chave} quantidade={g.quantidade} maximo={maximoGenero} />
          ))}
        </section>

        <section className="dashboard-bloco">
          <h2>Livros por autor</h2>
          {dados.porAutor.map((a) => (
            <BarraContagem key={a.chave} chave={a.chave} quantidade={a.quantidade} maximo={maximoAutor} />
          ))}
        </section>
      </div>

      {dados.ultimoAdicionado && (
        <section className="dashboard-ultimo">
          <h2>Último livro adicionado</h2>
          <Link to={`/livros/${dados.ultimoAdicionado.id}`} className="dashboard-ultimo-link">
            <strong>{dados.ultimoAdicionado.titulo}</strong> — {dados.ultimoAdicionado.autor}
          </Link>
        </section>
      )}
    </div>
  );
}

import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { criarDesafio, entrarNoDesafio, listarDesafios, sairDoDesafio } from "../services/desafioService";

const FORM_VAZIO = { titulo: "", descricao: "", dataInicio: "", dataFim: "" };

export default function Desafios() {
  const [desafios, setDesafios] = useState([]);
  const [carregando, setCarregando] = useState(true);
  const [mostrarForm, setMostrarForm] = useState(false);
  const [form, setForm] = useState(FORM_VAZIO);
  const [enviando, setEnviando] = useState(false);
  const [erro, setErro] = useState(null);

  function carregar() {
    listarDesafios().then((dados) => {
      setDesafios(dados);
      setCarregando(false);
    });
  }

  useEffect(carregar, []);

  function enviar(e) {
    e.preventDefault();
    setEnviando(true);
    setErro(null);
    criarDesafio(form)
      .then(() => {
        setForm(FORM_VAZIO);
        setMostrarForm(false);
        carregar();
      })
      .catch((err) => setErro(err.response?.data?.mensagem ?? "Não foi possível criar o desafio."))
      .finally(() => setEnviando(false));
  }

  function alternarParticipacao(desafio) {
    const acao = desafio.voceParticipa ? sairDoDesafio(desafio.id) : entrarNoDesafio(desafio.id);
    acao.then(carregar);
  }

  if (carregando) return <div className="carregando">Carregando</div>;

  return (
    <div className="pagina-desafios">
      <div className="pagina-desafios-cabecalho">
        <h1>Desafios de leitura</h1>
        <button type="button" onClick={() => setMostrarForm((v) => !v)}>
          {mostrarForm ? "Cancelar" : "Criar desafio"}
        </button>
      </div>

      {mostrarForm && (
        <form className="cartao-desafio-form" onSubmit={enviar}>
          <label>
            Título
            <input
              value={form.titulo}
              onChange={(e) => setForm({ ...form, titulo: e.target.value })}
              required
              minLength={3}
              maxLength={100}
            />
          </label>
          <label>
            Descrição (opcional)
            <textarea
              value={form.descricao}
              onChange={(e) => setForm({ ...form, descricao: e.target.value })}
              maxLength={1000}
              rows={3}
            />
          </label>
          <div className="form-desafio-datas">
            <label>
              Início
              <input
                type="date"
                value={form.dataInicio}
                onChange={(e) => setForm({ ...form, dataInicio: e.target.value })}
                required
              />
            </label>
            <label>
              Fim
              <input
                type="date"
                value={form.dataFim}
                onChange={(e) => setForm({ ...form, dataFim: e.target.value })}
                required
              />
            </label>
          </div>
          {erro && <p className="form-erro">{erro}</p>}
          <button type="submit" disabled={enviando}>
            {enviando ? "Criando..." : "Criar desafio"}
          </button>
        </form>
      )}

      {desafios.length === 0 ? (
        <p className="perfil-dica">Nenhum desafio criado ainda — crie o primeiro acima.</p>
      ) : (
        <div className="lista-desafios">
          {desafios.map((d) => (
            <div key={d.id} className="cartao-desafio">
              <div className="cartao-desafio-info">
                <div className="cartao-desafio-titulo-linha">
                  <Link to={`/desafios/${d.id}`}>
                    <strong>{d.titulo}</strong>
                  </Link>
                  <span className={`rotulo-status-desafio ${d.status === "Ativo" ? "ativo" : "encerrado"}`}>
                    {d.status}
                  </span>
                </div>
                <span className="cartao-desafio-meta">
                  {new Date(d.dataInicio).toLocaleDateString("pt-BR")} –{" "}
                  {new Date(d.dataFim).toLocaleDateString("pt-BR")} · criado por {d.criadorNomeUsuario} ·{" "}
                  {d.totalParticipantes} {d.totalParticipantes === 1 ? "participante" : "participantes"}
                </span>
              </div>
              {d.status === "Ativo" && (
                <button
                  type="button"
                  className={d.voceParticipa ? "btn-cancelar" : "btn-participar"}
                  onClick={() => alternarParticipacao(d)}
                >
                  {d.voceParticipa ? "Sair" : "Entrar"}
                </button>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

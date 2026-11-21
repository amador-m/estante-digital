import { useEffect, useState } from "react";

const GENEROS = ["Fantasia", "Suspense", "Romance", "Drama", "Aventura", "Biografia", "Outro"];
const STATUS = [
  { valor: "QueroLer", rotulo: "Quero ler" },
  { valor: "Lendo", rotulo: "Lendo" },
  { valor: "Lido", rotulo: "Lido" },
];

const VAZIO = {
  titulo: "",
  autor: "",
  serie: "",
  genero: "Fantasia",
  anoPublicacao: new Date().getFullYear(),
  sinopseCurta: "",
  numeroPaginas: "",
  statusLeitura: "QueroLer",
  avaliacao: "",
  paginaAtual: "",
  resenha: "",
};

export default function FormularioLivro({ livroEmEdicao, aoSalvar, aoCancelar, enviando, erro }) {
  const [form, setForm] = useState(VAZIO);

  useEffect(() => {
    if (livroEmEdicao) {
      setForm({
        titulo: livroEmEdicao.titulo,
        autor: livroEmEdicao.autor,
        serie: livroEmEdicao.serie ?? "",
        genero: livroEmEdicao.genero,
        anoPublicacao: livroEmEdicao.anoPublicacao,
        sinopseCurta: livroEmEdicao.sinopseCurta,
        numeroPaginas: livroEmEdicao.numeroPaginas ?? "",
        statusLeitura: livroEmEdicao.statusLeitura,
        avaliacao: livroEmEdicao.avaliacao ?? "",
        paginaAtual: livroEmEdicao.paginaAtual ?? "",
        resenha: livroEmEdicao.resenha ?? "",
      });
    } else {
      setForm(VAZIO);
    }
  }, [livroEmEdicao]);

  function campo(nome, valor) {
    setForm((atual) => ({ ...atual, [nome]: valor }));
  }

  function enviar(e) {
    e.preventDefault();

    aoSalvar({
      titulo: form.titulo,
      autor: form.autor,
      serie: form.serie.trim() === "" ? null : form.serie,
      genero: form.genero,
      anoPublicacao: Number(form.anoPublicacao),
      sinopseCurta: form.sinopseCurta,
      numeroPaginas: form.numeroPaginas === "" ? null : Number(form.numeroPaginas),
      statusLeitura: form.statusLeitura,
      avaliacao: form.avaliacao === "" ? null : Number(form.avaliacao),
      paginaAtual: form.paginaAtual === "" ? null : Number(form.paginaAtual),
      resenha: form.resenha.trim() === "" ? null : form.resenha,
    });
  }

  const podeAvaliar = form.statusLeitura === "Lido";

  return (
    <form className="form-livro" onSubmit={enviar}>
      <h2>{livroEmEdicao ? "Editar livro" : "Adicionar livro"}</h2>

      <div className="form-livro-grade">
        <label>
          Título
          <input value={form.titulo} onChange={(e) => campo("titulo", e.target.value)} required minLength={2} />
        </label>

        <label>
          Autor
          <input value={form.autor} onChange={(e) => campo("autor", e.target.value)} required minLength={2} />
        </label>

        <label>
          Série (opcional)
          <input value={form.serie} onChange={(e) => campo("serie", e.target.value)} />
        </label>

        <label>
          Gênero
          <select value={form.genero} onChange={(e) => campo("genero", e.target.value)}>
            {GENEROS.map((g) => (
              <option key={g} value={g}>
                {g}
              </option>
            ))}
          </select>
        </label>

        <label>
          Ano de publicação
          <input
            type="number"
            value={form.anoPublicacao}
            onChange={(e) => campo("anoPublicacao", e.target.value)}
            required
          />
        </label>

        <label>
          Número de páginas (opcional)
          <input
            type="number"
            value={form.numeroPaginas}
            onChange={(e) => campo("numeroPaginas", e.target.value)}
            min={1}
          />
        </label>

        <label>
          Página atual (opcional)
          <input
            type="number"
            value={form.paginaAtual}
            onChange={(e) => campo("paginaAtual", e.target.value)}
            min={0}
            max={form.numeroPaginas || undefined}
          />
        </label>

        <label>
          Status de leitura
          <select
            value={form.statusLeitura}
            onChange={(e) => {
              const novoStatus = e.target.value;
              campo("statusLeitura", novoStatus);
              if (novoStatus !== "Lido") campo("avaliacao", "");
            }}
          >
            {STATUS.map((s) => (
              <option key={s.valor} value={s.valor}>
                {s.rotulo}
              </option>
            ))}
          </select>
        </label>

        <label>
          Avaliação (1 a 5 — só se "Lido")
          <input
            type="number"
            min={1}
            max={5}
            value={form.avaliacao}
            disabled={!podeAvaliar}
            onChange={(e) => campo("avaliacao", e.target.value)}
          />
        </label>
      </div>

      <label className="form-livro-sinopse">
        Sinopse
        <textarea
          value={form.sinopseCurta}
          onChange={(e) => campo("sinopseCurta", e.target.value)}
          required
          minLength={10}
          rows={3}
        />
      </label>

      <label className="form-livro-sinopse">
        Resenha (opcional — visível para quem te segue)
        <textarea
          value={form.resenha}
          onChange={(e) => campo("resenha", e.target.value)}
          maxLength={4000}
          rows={4}
          placeholder="O que você achou do livro? Quem te segue vai ver isso no seu perfil."
        />
      </label>

      {erro && <p className="form-erro">{erro}</p>}

      <div className="form-livro-acoes">
        <button type="submit" disabled={enviando}>
          {enviando ? "Salvando..." : "Salvar"}
        </button>
        {livroEmEdicao && (
          <button type="button" className="btn-cancelar" onClick={aoCancelar} disabled={enviando}>
            Cancelar edição
          </button>
        )}
      </div>
    </form>
  );
}

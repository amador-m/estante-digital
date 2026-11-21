const GENEROS = ["Fantasia", "Suspense", "Romance", "Drama", "Aventura", "Biografia", "Outro"];
const STATUS = ["QueroLer", "Lendo", "Lido"];
const ROTULO_STATUS = { QueroLer: "Quero ler", Lendo: "Lendo", Lido: "Lido" };

export default function BarraDeFiltros({ filtros, aoMudar }) {
  function atualizar(campo, valor) {
    aoMudar({ ...filtros, [campo]: valor });
  }

  return (
    <div className="barra-filtros">
      <input
        className="filtro-busca"
        type="text"
        placeholder="Buscar por título, autor ou série..."
        value={filtros.busca}
        onChange={(e) => atualizar("busca", e.target.value)}
      />

      <select value={filtros.genero} onChange={(e) => atualizar("genero", e.target.value)}>
        <option value="">Todos os gêneros</option>
        {GENEROS.map((g) => (
          <option key={g} value={g}>
            {g}
          </option>
        ))}
      </select>

      <select value={filtros.status} onChange={(e) => atualizar("status", e.target.value)}>
        <option value="">Todos os status</option>
        {STATUS.map((s) => (
          <option key={s} value={s}>
            {ROTULO_STATUS[s]}
          </option>
        ))}
      </select>
    </div>
  );
}

import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import IconePerfil from "../components/IconePerfil";
import { obterPaleta } from "../constantes/perfil";
import { useTemaEscuro } from "../hooks/useTemaEscuro";
import { obterFeed } from "../services/feedService";

// Cada tipo de atividade monta sua própria frase a partir dos campos que o
// AtividadeDto traz pra aquele tipo (ver comentário em AtividadeDto.cs) —
// mesma lógica de "o front decide o que renderizar olhando pro tipo" que
// o resto do projeto já usa (StatusLeitura em LivroDto, por exemplo).
function frase(a) {
  switch (a.tipo) {
    case "TerminouLivro":
      return (
        <>
          <strong>{a.nomeUsuario}</strong> terminou de ler <strong>{a.livroTitulo}</strong>
        </>
      );
    case "ResenhaPublicada":
      return (
        <>
          <strong>{a.nomeUsuario}</strong> publicou uma resenha de <strong>{a.livroTitulo}</strong>
        </>
      );
    case "MetaBatida":
      return (
        <>
          <strong>{a.nomeUsuario}</strong> bateu a meta de <strong>{a.metaValor}</strong> livros em{" "}
          {new Date(a.criadoEm).getFullYear()}
        </>
      );
    case "CheckIn":
      return (
        <>
          <strong>{a.nomeUsuario}</strong> está numa sequência de <strong>{a.sequenciaDias}</strong>{" "}
          {a.sequenciaDias === 1 ? "dia" : "dias"} lendo
        </>
      );
    case "DesafioConcluido":
      return (
        <>
          <strong>{a.nomeUsuario}</strong> terminou o desafio <strong>{a.desafioTitulo}</strong> com{" "}
          {a.livrosNoDesafio} {a.livrosNoDesafio === 1 ? "livro" : "livros"} lidos
        </>
      );
    default:
      return <strong>{a.nomeUsuario}</strong>;
  }
}

export default function Feed() {
  const [temaEscuro] = useTemaEscuro();
  const [atividades, setAtividades] = useState(null);

  useEffect(() => {
    obterFeed().then(setAtividades);
  }, []);

  if (!atividades) return <div className="carregando">Carregando</div>;

  return (
    <div className="pagina-feed">
      <h1>Feed</h1>

      {atividades.length === 0 ? (
        <p className="perfil-dica">
          Nada por aqui ainda. Siga pessoas em <Link to="/pessoas">Usuários</Link> pra ver as atividades delas.
        </p>
      ) : (
        <div className="lista-feed">
          {atividades.map((a) => (
            <div key={a.id} className="item-feed">
              <IconePerfil
                icone={a.icone}
                tamanho={36}
                corFundo={obterPaleta(a.corPrincipal)[temaEscuro ? "escuro" : "claro"].acento}
              />
              <div className="item-feed-texto">
                {frase(a)}
                <span className="item-feed-quando">
                  {new Date(a.criadoEm).toLocaleDateString("pt-BR")} às{" "}
                  {new Date(a.criadoEm).toLocaleTimeString("pt-BR", { hour: "2-digit", minute: "2-digit" })}
                </span>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

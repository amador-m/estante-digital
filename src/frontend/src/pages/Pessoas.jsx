import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import IconePerfil from "../components/IconePerfil";
import { buscarUsuarios, listarSeguidores, listarSeguindo } from "../services/perfilSocialService";

function ListaUsuarios({ usuarios, vazio }) {
  if (usuarios.length === 0) {
    return <p className="perfil-dica">{vazio}</p>;
  }

  return (
    <div className="lista-pessoas">
      {usuarios.map((u) => (
        <Link key={u.id} to={`/pessoas/${u.nomeUsuario}`} className="cartao-pessoa">
          <IconePerfil icone={u.icone} tamanho={40} />
          <span>{u.nomeUsuario}</span>
        </Link>
      ))}
    </div>
  );
}

export default function Pessoas() {
  const [termo, setTermo] = useState("");
  const [resultados, setResultados] = useState([]);
  const [buscando, setBuscando] = useState(false);
  const [erroBusca, setErroBusca] = useState(null);

  const [seguindo, setSeguindo] = useState([]);
  const [seguidores, setSeguidores] = useState([]);

  useEffect(() => {
    listarSeguindo().then(setSeguindo);
    listarSeguidores().then(setSeguidores);
  }, []);

  useEffect(() => {
    if (termo.trim().length < 2) {
      setResultados([]);
      setErroBusca(null);
      return;
    }

    let cancelado = false;
    setBuscando(true);
    const timeout = setTimeout(() => {
      buscarUsuarios(termo.trim())
        .then((dados) => {
          if (!cancelado) {
            setResultados(dados);
            setErroBusca(null);
          }
        })
        .catch((e) => {
          if (!cancelado) setErroBusca(e.response?.data?.mensagem ?? "Não foi possível buscar.");
        })
        .finally(() => {
          if (!cancelado) setBuscando(false);
        });
    }, 300);

    return () => {
      cancelado = true;
      clearTimeout(timeout);
    };
  }, [termo]);

  return (
    <div className="pagina-pessoas">
      <h1>Pessoas</h1>

      <label className="busca-pessoas">
        Buscar por nome de usuário
        <input
          value={termo}
          onChange={(e) => setTermo(e.target.value)}
          placeholder="Digite pelo menos 2 letras..."
        />
      </label>

      {buscando && <p className="perfil-dica">Buscando...</p>}
      {erroBusca && <p className="form-erro">{erroBusca}</p>}
      {termo.trim().length >= 2 && !buscando && !erroBusca && (
        <ListaUsuarios usuarios={resultados} vazio="Ninguém encontrado com esse nome." />
      )}

      <div className="pessoas-colunas">
        <section>
          <h2>Seguindo ({seguindo.length})</h2>
          <ListaUsuarios usuarios={seguindo} vazio="Você ainda não segue ninguém." />
        </section>

        <section>
          <h2>Seguidores ({seguidores.length})</h2>
          <ListaUsuarios usuarios={seguidores} vazio="Ninguém te segue ainda." />
        </section>
      </div>
    </div>
  );
}

import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import CartaoLivroPublico from "../components/CartaoLivroPublico";
import IconePerfil from "../components/IconePerfil";
import { obterPaleta } from "../constantes/perfil";
import { useTemaEscuro } from "../hooks/useTemaEscuro";
import { deixarDeSeguir, obterPerfilPublico, seguir } from "../services/perfilSocialService";

export default function PerfilPublico() {
  const { nomeUsuario } = useParams();
  const [temaEscuro] = useTemaEscuro();

  const [perfil, setPerfil] = useState(null);
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState(null);
  const [alternandoSeguir, setAlternandoSeguir] = useState(false);

  function carregar() {
    setCarregando(true);
    setErro(null);
    obterPerfilPublico(nomeUsuario)
      .then(setPerfil)
      .catch((e) => setErro(e.response?.data?.mensagem ?? "Usuário não encontrado."))
      .finally(() => setCarregando(false));
  }

  useEffect(carregar, [nomeUsuario]);

  async function alternarSeguir() {
    setAlternandoSeguir(true);
    try {
      if (perfil.voceSegue) {
        await deixarDeSeguir(nomeUsuario);
      } else {
        await seguir(nomeUsuario);
      }
      carregar();
    } finally {
      setAlternandoSeguir(false);
    }
  }

  if (carregando) return <div className="carregando">Carregando perfil...</div>;
  if (erro || !perfil) return <div className="erro-api">{erro ?? "Usuário não encontrado."}</div>;

  const variantePaleta = obterPaleta(perfil.corPrincipal)[temaEscuro ? "escuro" : "claro"];
  const anoAtual = new Date().getFullYear();

  return (
    <div className="pagina-perfil-publico">
      <Link to="/pessoas" className="link-voltar">
        ← Voltar
      </Link>

      <div className="cartao-perfil cartao-perfil-publico-cabecalho">
        <div className="perfil-preview">
          <IconePerfil icone={perfil.icone} tamanho={56} corFundo={variantePaleta.acento} />
          <div>
            <strong>{perfil.nomeUsuario}</strong>
            {perfil.ehVoceMesmo && <span className="perfil-dica">Este é o seu perfil</span>}
          </div>
        </div>

        {!perfil.ehVoceMesmo && (
          <button
            type="button"
            className={perfil.voceSegue ? "btn-cancelar" : ""}
            onClick={alternarSeguir}
            disabled={alternandoSeguir}
          >
            {alternandoSeguir ? "Aguarde..." : perfil.voceSegue ? "Deixar de seguir" : "Seguir"}
          </button>
        )}

        <div className="perfil-publico-estatisticas">
          <div>
            <strong>{perfil.totalSeguidores}</strong>
            <span>seguidores</span>
          </div>
          <div>
            <strong>{perfil.totalSeguindo}</strong>
            <span>seguindo</span>
          </div>
          {perfil.metaLeituraAnual != null && (
            <div>
              <strong>
                {perfil.livrosLidosNoAnoAtual}/{perfil.metaLeituraAnual}
              </strong>
              <span>livros em {anoAtual}</span>
            </div>
          )}
        </div>
      </div>

      {perfil.ehVoceMesmo || perfil.voceSegue ? (
        perfil.estante.length === 0 ? (
          <p className="perfil-dica">A estante de {perfil.nomeUsuario} ainda está vazia.</p>
        ) : (
          <div className="grade-livros">
            {perfil.estante.map((livro) => (
              <CartaoLivroPublico key={livro.id} livro={livro} />
            ))}
          </div>
        )
      ) : (
        <p className="perfil-dica">Siga {perfil.nomeUsuario} para ver a estante completa (livros, status e resenhas).</p>
      )}
    </div>
  );
}

import { useContext } from "react";
import { NavLink, useNavigate } from "react-router-dom";
import { AuthContext } from "../context/AuthContext";
import { useTemaEscuro } from "../hooks/useTemaEscuro";
import { useCorPrincipal } from "../hooks/useCorPrincipal";
import { COR_PADRAO } from "../constantes/perfil";
import LogoEstante from "./LogoEstante";
import IconePerfil from "./IconePerfil";

export default function BarraSuperior() {
  const { usuario, perfil, isAuth, logout } = useContext(AuthContext);
  const navigate = useNavigate();
  const [temaEscuro, alternarTema] = useTemaEscuro();

  // Fica aqui (não em cada página) porque BarraSuperior é a única coisa
  // presente em toda rota (ver Layout em App.jsx) — um só lugar aplicando
  // a cor da conta, reagindo tanto à troca de cor quanto à troca de tema.
  useCorPrincipal(perfil?.corPrincipal ?? COR_PADRAO, temaEscuro);

  async function sair() {
    await logout();
    navigate("/login");
  }

  return (
    <header className="barra-superior">
      <div className="barra-marca">
        <LogoEstante tamanho={30} />
        <span className="barra-marca-texto">Estante Digital</span>
      </div>

      <nav className="barra-nav">
        <NavLink to="/" end className={({ isActive }) => (isActive ? "ativo" : "")}>
          Meus Livros
        </NavLink>
        <NavLink to="/dashboard" className={({ isActive }) => (isActive ? "ativo" : "")}>
          Visão Geral
        </NavLink>
        <NavLink to="/ranking" className={({ isActive }) => (isActive ? "ativo" : "")}>
          Ranking de Leituras
        </NavLink>
        {isAuth && (
          <NavLink to="/gerenciar" className={({ isActive }) => (isActive ? "ativo" : "")}>
            Gerenciar Livros
          </NavLink>
        )}
        {isAuth && (
          <NavLink to="/pessoas" className={({ isActive }) => (isActive ? "ativo" : "")}>
            Usuários
          </NavLink>
        )}
      </nav>

      <div className="barra-usuario">
        <button
          className="btn-tema"
          onClick={alternarTema}
          type="button"
          aria-label={temaEscuro ? "Mudar para tema claro" : "Mudar para tema escuro"}
          title={temaEscuro ? "Mudar para tema claro" : "Mudar para tema escuro"}
        >
          {temaEscuro ? "☀️" : "🌙"}
        </button>
        {isAuth ? (
          <>
            <NavLink to="/perfil" className="barra-perfil-link" title="Meu perfil">
              <IconePerfil icone={perfil?.icone ?? "livro-aberto"} tamanho={26} />
              <span className="barra-email">{perfil?.nomeUsuario ?? usuario?.email}</span>
            </NavLink>
            <button className="btn-sair" onClick={sair}>
              Sair
            </button>
          </>
        ) : (
          <>
            <NavLink to="/cadastro" className="btn-cadastrar">
              Cadastrar
            </NavLink>
            <NavLink to="/login" className="btn-entrar">
              Entrar
            </NavLink>
          </>
        )}
      </div>
    </header>
  );
}

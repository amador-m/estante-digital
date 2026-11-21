import { useContext, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { AuthContext } from "../context/AuthContext";

export default function Login() {
  const { login } = useContext(AuthContext);
  const navigate = useNavigate();

  const [email, setEmail] = useState("");
  const [senha, setSenha] = useState("");
  const [erro, setErro] = useState(null);
  const [enviando, setEnviando] = useState(false);

  async function enviar(e) {
    e.preventDefault();
    setErro(null);
    setEnviando(true);

    try {
      await login(email, senha);
      navigate("/");
    } catch (erroRequisicao) {
      if (erroRequisicao.response?.status === 401) {
        setErro("E-mail ou senha inválidos.");
      } else {
        setErro("Não foi possível conectar à API. Ela está rodando?");
      }
    } finally {
      setEnviando(false);
    }
  }

  return (
    <div className="pagina-login">
      <form className="form-login" onSubmit={enviar}>
        <h1>Entrar</h1>
        <p className="login-subtitulo">Cada conta tem sua própria estante, privada.</p>

        <input
          type="email"
          placeholder="E-mail"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          required
        />

        <input
          type="password"
          placeholder="Senha"
          value={senha}
          onChange={(e) => setSenha(e.target.value)}
          required
        />

        <button type="submit" disabled={enviando}>
          {enviando ? "Entrando..." : "Entrar"}
        </button>

        {erro && <p className="form-erro">{erro}</p>}

        <p className="login-dica">
          Conta de demonstração (estante já preenchida): <code>demo@estantedigital.com</code> /{" "}
          <code>Estante@123</code>
        </p>

        <p className="login-cadastro">
          Não tem conta? <Link to="/cadastro">Cadastre-se</Link>
        </p>
      </form>
    </div>
  );
}

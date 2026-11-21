import { useContext, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { AuthContext } from "../context/AuthContext";

export default function Registro() {
  const { registrar } = useContext(AuthContext);
  const navigate = useNavigate();

  const [email, setEmail] = useState("");
  const [senha, setSenha] = useState("");
  const [confirmarSenha, setConfirmarSenha] = useState("");
  const [erro, setErro] = useState(null);
  const [enviando, setEnviando] = useState(false);

  async function enviar(e) {
    e.preventDefault();
    setErro(null);

    if (senha !== confirmarSenha) {
      setErro("As senhas não coincidem.");
      return;
    }

    setEnviando(true);
    try {
      await registrar(email, senha);
      navigate("/");
    } catch (erroRequisicao) {
      setErro(erroRequisicao.response?.data?.mensagem ?? "Não foi possível criar a conta.");
    } finally {
      setEnviando(false);
    }
  }

  return (
    <div className="pagina-login">
      <form className="form-login" onSubmit={enviar}>
        <h1>Criar conta</h1>
        <p className="login-subtitulo">Sua estante começa vazia — você monta o acervo do seu jeito.</p>

        <input
          type="email"
          placeholder="E-mail"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          required
        />

        <input
          type="password"
          placeholder="Senha (mínimo 6 caracteres)"
          value={senha}
          onChange={(e) => setSenha(e.target.value)}
          minLength={6}
          required
        />

        <input
          type="password"
          placeholder="Confirmar senha"
          value={confirmarSenha}
          onChange={(e) => setConfirmarSenha(e.target.value)}
          minLength={6}
          required
        />

        <button type="submit" disabled={enviando}>
          {enviando ? "Criando conta..." : "Criar conta"}
        </button>

        {erro && <p className="form-erro">{erro}</p>}

        <p className="login-cadastro">
          Já tem conta? <Link to="/login">Entrar</Link>
        </p>
      </form>
    </div>
  );
}

import { useContext, useEffect, useState } from "react";
import { AuthContext } from "../context/AuthContext";
import { atualizarPerfil, alterarSenha } from "../services/perfilService";
import { ICONES_PERFIL, PALETA_CORES } from "../constantes/perfil";
import IconePerfil from "../components/IconePerfil";

export default function Perfil() {
  const { perfil, setPerfil } = useContext(AuthContext);

  const [nomeUsuario, setNomeUsuario] = useState("");
  const [icone, setIcone] = useState("livro-aberto");
  const [corPrincipal, setCorPrincipal] = useState("azul");
  const [metaLeituraAnual, setMetaLeituraAnual] = useState("");
  const [salvando, setSalvando] = useState(false);
  const [erroPerfil, setErroPerfil] = useState(null);
  const [sucessoPerfil, setSucessoPerfil] = useState(false);

  const [senhaAtual, setSenhaAtual] = useState("");
  const [novaSenha, setNovaSenha] = useState("");
  const [confirmarNovaSenha, setConfirmarNovaSenha] = useState("");
  const [trocandoSenha, setTrocandoSenha] = useState(false);
  const [erroSenha, setErroSenha] = useState(null);
  const [sucessoSenha, setSucessoSenha] = useState(false);

  useEffect(() => {
    if (perfil) {
      setNomeUsuario(perfil.nomeUsuario);
      setIcone(perfil.icone);
      setCorPrincipal(perfil.corPrincipal);
      setMetaLeituraAnual(perfil.metaLeituraAnual ?? "");
    }
  }, [perfil]);

  async function salvarPerfil(e) {
    e.preventDefault();
    setErroPerfil(null);
    setSucessoPerfil(false);
    setSalvando(true);

    try {
      const atualizado = await atualizarPerfil({
        nomeUsuario,
        icone,
        corPrincipal,
        metaLeituraAnual: metaLeituraAnual === "" ? null : Number(metaLeituraAnual),
      });
      setPerfil(atualizado);
      setSucessoPerfil(true);
    } catch (erro) {
      setErroPerfil(erro.response?.data?.mensagem ?? "Não foi possível salvar o perfil.");
    } finally {
      setSalvando(false);
    }
  }

  async function enviarTrocaSenha(e) {
    e.preventDefault();
    setErroSenha(null);
    setSucessoSenha(false);

    if (novaSenha !== confirmarNovaSenha) {
      setErroSenha("As senhas novas não coincidem.");
      return;
    }

    setTrocandoSenha(true);
    try {
      await alterarSenha({ senhaAtual, novaSenha });
      setSucessoSenha(true);
      setSenhaAtual("");
      setNovaSenha("");
      setConfirmarNovaSenha("");
    } catch (erro) {
      setErroSenha(erro.response?.data?.mensagem ?? "Não foi possível trocar a senha.");
    } finally {
      setTrocandoSenha(false);
    }
  }

  if (!perfil) {
    return <div className="carregando">Carregando perfil...</div>;
  }

  return (
    <div className="pagina-perfil">
      <h1>Meu perfil</h1>

      <form className="cartao-perfil" onSubmit={salvarPerfil}>
        <h2>Identidade</h2>

        <div className="perfil-preview">
          <IconePerfil icone={icone} tamanho={56} />
          <div>
            <strong>{nomeUsuario || perfil.nomeUsuario}</strong>
            <span className="perfil-email">{perfil.email}</span>
          </div>
        </div>

        <label>
          Nome de usuário
          <input
            value={nomeUsuario}
            onChange={(e) => setNomeUsuario(e.target.value)}
            minLength={3}
            maxLength={24}
            required
          />
        </label>
        <p className="perfil-dica">Letras, números, ponto, traço e underline — entre 3 e 24 caracteres.</p>

        <div className="perfil-campo-grupo">
          <span className="perfil-rotulo-grupo">Ícone</span>
          <div className="grade-icones">
            {ICONES_PERFIL.map((opcao) => (
              <button
                type="button"
                key={opcao.valor}
                className={"opcao-icone" + (icone === opcao.valor ? " selecionado" : "")}
                onClick={() => setIcone(opcao.valor)}
                title={opcao.rotulo}
                aria-label={opcao.rotulo}
              >
                <IconePerfil icone={opcao.valor} tamanho={36} />
              </button>
            ))}
          </div>
        </div>

        <div className="perfil-campo-grupo">
          <span className="perfil-rotulo-grupo">Cor principal</span>
          <div className="grade-cores">
            {PALETA_CORES.map((cor) => (
              <button
                type="button"
                key={cor.valor}
                className={"opcao-cor" + (corPrincipal === cor.valor ? " selecionado" : "")}
                style={{ backgroundColor: cor.claro.acento }}
                onClick={() => setCorPrincipal(cor.valor)}
                title={cor.rotulo}
                aria-label={cor.rotulo}
              />
            ))}
          </div>
        </div>

        <div className="perfil-campo-grupo">
          <span className="perfil-rotulo-grupo">Meta de leitura anual (opcional)</span>
          <input
            type="number"
            min={0}
            max={1000}
            value={metaLeituraAnual}
            onChange={(e) => setMetaLeituraAnual(e.target.value)}
            placeholder="Ex.: 20 livros em 2026"
          />
          <p className="perfil-dica">Vale só pro ano corrente — mude quando quiser, inclusive pro próximo ano.</p>
        </div>

        {erroPerfil && <p className="form-erro">{erroPerfil}</p>}
        {sucessoPerfil && <p className="form-sucesso">Perfil atualizado.</p>}

        <button type="submit" disabled={salvando}>
          {salvando ? "Salvando..." : "Salvar perfil"}
        </button>
      </form>

      <form className="cartao-perfil" onSubmit={enviarTrocaSenha}>
        <h2>Trocar senha</h2>

        <label>
          Senha atual
          <input type="password" value={senhaAtual} onChange={(e) => setSenhaAtual(e.target.value)} required />
        </label>

        <label>
          Nova senha
          <input
            type="password"
            value={novaSenha}
            onChange={(e) => setNovaSenha(e.target.value)}
            minLength={6}
            required
          />
        </label>

        <label>
          Confirmar nova senha
          <input
            type="password"
            value={confirmarNovaSenha}
            onChange={(e) => setConfirmarNovaSenha(e.target.value)}
            minLength={6}
            required
          />
        </label>

        {erroSenha && <p className="form-erro">{erroSenha}</p>}
        {sucessoSenha && <p className="form-sucesso">Senha alterada.</p>}

        <button type="submit" disabled={trocandoSenha}>
          {trocandoSenha ? "Trocando..." : "Trocar senha"}
        </button>
      </form>
    </div>
  );
}

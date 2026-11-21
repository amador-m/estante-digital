import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom";
import BarraSuperior from "./components/BarraSuperior";
import PrivateRoute from "./components/PrivateRoute";
import Catalogo from "./pages/Catalogo";
import DetalheLivro from "./pages/DetalheLivro";
import Dashboard from "./pages/Dashboard";
import Ranking from "./pages/Ranking";
import Login from "./pages/Login";
import Registro from "./pages/Registro";
import Gerenciar from "./pages/Gerenciar";
import Perfil from "./pages/Perfil";
import Pessoas from "./pages/Pessoas";
import PerfilPublico from "./pages/PerfilPublico";

function Layout({ children }) {
  return (
    <div className="app">
      <BarraSuperior />
      <main className="conteudo">{children}</main>
    </div>
  );
}

// Fase 2: como cada estante agora é privada por conta, só /login e
// /cadastro ficam fora do PrivateRoute — todo o resto (catálogo, detalhe,
// dashboard, ranking, gerenciar) exige sessão.
export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route
          path="/"
          element={
            <PrivateRoute>
              <Layout>
                <Catalogo />
              </Layout>
            </PrivateRoute>
          }
        />
        <Route
          path="/livros/:id"
          element={
            <PrivateRoute>
              <Layout>
                <DetalheLivro />
              </Layout>
            </PrivateRoute>
          }
        />
        <Route
          path="/dashboard"
          element={
            <PrivateRoute>
              <Layout>
                <Dashboard />
              </Layout>
            </PrivateRoute>
          }
        />
        <Route
          path="/ranking"
          element={
            <PrivateRoute>
              <Layout>
                <Ranking />
              </Layout>
            </PrivateRoute>
          }
        />
        <Route
          path="/login"
          element={
            <Layout>
              <Login />
            </Layout>
          }
        />
        <Route
          path="/cadastro"
          element={
            <Layout>
              <Registro />
            </Layout>
          }
        />
        <Route
          path="/gerenciar"
          element={
            <PrivateRoute>
              <Layout>
                <Gerenciar />
              </Layout>
            </PrivateRoute>
          }
        />
        <Route
          path="/perfil"
          element={
            <PrivateRoute>
              <Layout>
                <Perfil />
              </Layout>
            </PrivateRoute>
          }
        />
        <Route
          path="/pessoas"
          element={
            <PrivateRoute>
              <Layout>
                <Pessoas />
              </Layout>
            </PrivateRoute>
          }
        />
        <Route
          path="/pessoas/:nomeUsuario"
          element={
            <PrivateRoute>
              <Layout>
                <PerfilPublico />
              </Layout>
            </PrivateRoute>
          }
        />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </BrowserRouter>
  );
}

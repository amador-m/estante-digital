import { useContext } from "react";
import { Navigate } from "react-router-dom";
import { AuthContext } from "../context/AuthContext";

export default function PrivateRoute({ children }) {
  const { isAuth, carregando } = useContext(AuthContext);

  if (carregando) {
    return <div className="verificando-sessao">Verificando sessão...</div>;
  }

  return isAuth ? children : <Navigate to="/login" replace />;
}

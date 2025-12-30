import { Navigate, Route, Routes } from "react-router-dom";
import { LoginPage } from "./pages/LoginPage";
import { QuotesPage } from "./pages/QuotesPage";
import { Layout } from "./components/Layout";
import { useAuth } from "./context/AuthContext";

function ProtectedQuotesRoute() {
  const auth = useAuth();

  if (!auth.state.isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  return <QuotesPage />;
}

function HomeRedirect() {
  const auth = useAuth();
  return auth.state.isAuthenticated ? (
    <Navigate to="/quotes" replace />
  ) : (
    <Navigate to="/login" replace />
  );
}

export function AppRouter() {
  return (
    <Layout>
      <Routes>
        <Route path="/" element={<HomeRedirect />} />
        <Route path="/login" element={<LoginPage />} />
        <Route path="/quotes" element={<ProtectedQuotesRoute />} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </Layout>
  );
}

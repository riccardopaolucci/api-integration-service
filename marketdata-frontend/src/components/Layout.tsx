import { type ReactNode } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

export function Layout({ children }: { children: ReactNode }) {
  const auth = useAuth();
  const navigate = useNavigate();

  function handleLogout() {
    auth.logout();
    navigate("/login");
  }

  return (
    <div style={{ maxWidth: 900, margin: "0 auto", padding: 16 }}>
      <header
        style={{
          display: "flex",
          alignItems: "center",
          justifyContent: "space-between",
          marginBottom: 24,
          borderBottom: "1px solid #eee",
          paddingBottom: 12,
        }}
      >
        <div style={{ display: "flex", gap: 12, alignItems: "center" }}>
          <strong>MarketData</strong>
          <nav style={{ display: "flex", gap: 12 }}>
            <Link to="/login">Login</Link>
            <Link to="/quotes">Quotes</Link>
          </nav>
        </div>

        {auth.state.isAuthenticated ? (
          <button onClick={handleLogout}>Log out</button>
        ) : null}
      </header>

      <main>{children}</main>
    </div>
  );
}

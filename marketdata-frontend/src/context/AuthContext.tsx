/* eslint-disable react-refresh/only-export-components */

import {
  createContext,
  useContext,
  useState,
  type ReactNode,
} from "react";
import * as authApi from "../api/auth";

export type AuthState = {
  token: string | null;
  isAuthenticated: boolean;
  username?: string;
  roles?: string[];
};

export type AuthContextValue = {
  state: AuthState;
  login: (username: string, password: string) => Promise<void>;
  logout: () => void;

  // test helper (safe to remove later)
  setTokenForTest: (token: string) => void;
};

const TOKEN_KEY = "marketdata_token";

/**
 * Exported so tests can use <AuthContext.Provider />
 */
export const AuthContext = createContext<AuthContextValue | undefined>(
  undefined
);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [state, setState] = useState<AuthState>(() => {
    const token = localStorage.getItem(TOKEN_KEY);
    return {
      token,
      isAuthenticated: Boolean(token),
    };
  });

  async function login(username: string, password: string) {
    const res = await authApi.login({ username, password });

    localStorage.setItem(TOKEN_KEY, res.token);

    setState({
      token: res.token,
      isAuthenticated: true,
      username: res.username,
      roles: res.roles,
    });
  }

  function logout() {
    localStorage.removeItem(TOKEN_KEY);
    setState({
      token: null,
      isAuthenticated: false,
    });
  }

  function setTokenForTest(token: string) {
    localStorage.setItem(TOKEN_KEY, token);
    setState({
      token,
      isAuthenticated: true,
    });
  }

  return (
    <AuthContext.Provider
      value={{ state, login, logout, setTokenForTest }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return ctx;
}

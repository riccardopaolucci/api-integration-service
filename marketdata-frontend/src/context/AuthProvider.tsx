import React, { useMemo, useState } from "react";
import { login as loginApi } from "../api/auth";
import { AuthContext, type AuthContextValue, type AuthState } from "./authContext";

const TOKEN_KEY = "marketdata_token";

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [token, setToken] = useState<string | null>(() => {
    return localStorage.getItem(TOKEN_KEY);
  });

  const state: AuthState = useMemo(
    () => ({
      token,
      isAuthenticated: !!token
    }),
    [token]
  );

  async function login(username: string, password: string) {
    const result = await loginApi({ username, password });
    localStorage.setItem(TOKEN_KEY, result.token);
    setToken(result.token);
  }

  function logout() {
    localStorage.removeItem(TOKEN_KEY);
    setToken(null);
  }

  function setTokenForTest(next: string) {
    localStorage.setItem(TOKEN_KEY, next);
    setToken(next);
  }

  const value: AuthContextValue = {
    state,
    login,
    logout,
    setTokenForTest
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

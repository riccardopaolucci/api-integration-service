import { createContext, useContext } from "react";

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

  // small helper for the first test (remove later if you want)
  setTokenForTest: (token: string) => void;
};

export const AuthContext = createContext<AuthContextValue | undefined>(
  undefined
);

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return ctx;
}

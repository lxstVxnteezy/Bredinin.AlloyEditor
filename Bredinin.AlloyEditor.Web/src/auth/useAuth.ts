import { createContext, useContext } from "react";

export interface AuthContextValue {
  status: "loading" | "authenticated" | "anonymous";
  roles: string[];
  userId: string | null;
  login(l: string, p: string): Promise<void>;
  logout(): void;
  isAdmin: boolean;
}

export const AuthContext = createContext<AuthContextValue | null>(null);

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used within AuthProvider");
  return ctx;
}

import { useCallback, useEffect, useLayoutEffect, useMemo, useRef, useState, type ReactNode } from "react";
import { login as apiLogin, refreshToken as apiRefresh, setAuthHandlers, type AuthResponse } from "../api";
import { AuthContext, type AuthContextValue } from "./useAuth";
import { parseJwt } from "./jwt";
import { clearRefreshToken, getRefreshToken, setRefreshToken } from "./tokenStorage";

// Role name from identity seed migration (2025_08_25_2116_InsertValuesInTables.cs): "admin".
const ADMIN_ROLE = "admin";

interface Session {
  roles: string[];
  userId: string | null;
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const accessRef = useRef<string | null>(null);
  const [status, setStatus] = useState<AuthContextValue["status"]>(() =>
    getRefreshToken() ? "loading" : "anonymous",
  );
  const [session, setSession] = useState<Session>({ roles: [], userId: null });

  const apply = useCallback((r: AuthResponse) => {
    accessRef.current = r.accessToken;
    setRefreshToken(r.refreshToken);
    setSession(parseJwt(r.accessToken));
    setStatus("authenticated");
  }, []);

  const reset = useCallback(() => {
    accessRef.current = null;
    clearRefreshToken();
    setSession({ roles: [], userId: null });
    setStatus("anonymous");
  }, []);

  // Layout effects run before child passive effects, so children can already use the API.
  useLayoutEffect(() => {
    setAuthHandlers({
      getAccessToken: () => accessRef.current,
      refresh: async () => {
        const stored = getRefreshToken();
        if (!stored) throw new Error("No refresh token");
        const r = await apiRefresh(stored);
        apply(r);
        return r.accessToken;
      },
      onAuthFailure: reset,
    });
  }, [apply, reset]);

  useEffect(() => {
    const stored = getRefreshToken();
    if (!stored) return;
    let cancelled = false;
    apiRefresh(stored)
      .then((r) => {
        if (!cancelled) apply(r);
      })
      .catch(() => {
        if (!cancelled) reset();
      });
    return () => {
      cancelled = true;
    };
  }, [apply, reset]);

  const value = useMemo<AuthContextValue>(
    () => ({
      status,
      roles: session.roles,
      userId: session.userId,
      isAdmin: session.roles.some((r) => r.toLowerCase() === ADMIN_ROLE),
      login: async (l, p) => apply(await apiLogin(l, p)),
      logout: reset,
    }),
    [status, session, apply, reset],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

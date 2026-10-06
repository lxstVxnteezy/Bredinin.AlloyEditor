import { useCallback, useEffect, useLayoutEffect, useMemo, useRef, useState, type ReactNode } from "react";
import { login as apiLogin, refreshToken as apiRefresh, setAuthHandlers, type AuthResponse } from "../api";
import { AuthContext, type AuthContextValue } from "./useAuth";
import { parseJwt } from "./jwt";
import { clearRefreshToken, getRefreshToken, setRefreshToken } from "./tokenStorage";

// Role name from identity seed migration (2025_08_25_2116_InsertValuesInTables.cs): "admin".
const ADMIN_ROLE = "admin";

let inflight: Promise<AuthResponse> | null = null;

// Single flight over the stored refresh token: refresh tokens rotate on the backend,
// so the mount exchange and 401-triggered refreshes must share one request.
function exchangeStored(): Promise<AuthResponse> {
  inflight ??= (async () => {
    const stored = getRefreshToken();
    if (!stored) throw new Error("No refresh token");
    return apiRefresh(stored);
  })().finally(() => {
    inflight = null;
  });
  return inflight;
}

interface Session {
  roles: string[];
  userId: string | null;
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const accessRef = useRef<string | null>(null);
  const genRef = useRef(0);
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
    genRef.current++;
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
        const gen = genRef.current;
        const r = await exchangeStored();
        if (gen !== genRef.current) throw new Error("Signed out during refresh");
        apply(r);
        return r.accessToken;
      },
      onAuthFailure: reset,
    });
  }, [apply, reset]);

  useEffect(() => {
    if (!getRefreshToken()) return;
    const gen = genRef.current;
    exchangeStored()
      .then((r) => {
        if (gen === genRef.current) apply(r);
      })
      .catch(() => {
        if (gen === genRef.current) reset();
      });
  }, [apply, reset]);

  const value = useMemo<AuthContextValue>(
    () => ({
      status,
      roles: session.roles,
      userId: session.userId,
      isAdmin: session.roles.some((r) => r.toLowerCase() === ADMIN_ROLE),
      login: async (l, p) => {
        const gen = genRef.current;
        const r = await apiLogin(l, p);
        if (gen === genRef.current) apply(r);
      },
      logout: reset,
    }),
    [status, session, apply, reset],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

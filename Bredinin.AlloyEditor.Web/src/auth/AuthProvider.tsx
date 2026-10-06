import { useCallback, useEffect, useLayoutEffect, useMemo, useRef, useState, type ReactNode } from "react";
import { login as apiLogin, refreshToken as apiRefresh, setAuthHandlers, type AuthResponse } from "../api";
import { AuthContext, type AuthContextValue } from "./useAuth";
import { parseJwt } from "./jwt";
import { ApiError } from "../api/errors";
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
  login: string | null;
}

const EMPTY_SESSION: Session = { roles: [], userId: null, login: null };

// Only an explicit rejection by the refresh endpoint invalidates the stored token;
// network errors, 5xx, 429 etc. are transient and must not log the user out for good.
class SupersededError extends Error {
  constructor() {
    super("Session changed during refresh");
  }
}

const isTokenRejected = (e: unknown): boolean => e instanceof ApiError && (e.status === 400 || e.status === 401);

interface Props {
  children: ReactNode;
  /** Called whenever a session ends (logout or forced logout), e.g. to clear cached data. */
  onSessionEnd?: () => void;
}

export function AuthProvider({ children, onSessionEnd }: Props) {
  const onSessionEndRef = useRef(onSessionEnd);
  useLayoutEffect(() => {
    onSessionEndRef.current = onSessionEnd;
  }, [onSessionEnd]);
  const accessRef = useRef<string | null>(null);
  const genRef = useRef(0);
  const [status, setStatus] = useState<AuthContextValue["status"]>(() =>
    getRefreshToken() ? "loading" : "anonymous",
  );
  const [session, setSession] = useState<Session>(EMPTY_SESSION);

  const apply = useCallback((r: AuthResponse) => {
    accessRef.current = r.accessToken;
    setRefreshToken(r.refreshToken);
    setSession(parseJwt(r.accessToken));
    setStatus("authenticated");
  }, []);

  const endSession = useCallback((forgetToken: boolean) => {
    genRef.current++;
    accessRef.current = null;
    if (forgetToken) clearRefreshToken();
    setSession(EMPTY_SESSION);
    setStatus("anonymous");
    onSessionEndRef.current?.();
  }, []);

  const reset = useCallback(() => endSession(true), [endSession]);

  // Refresh failed: forget the token only if the server rejected it, otherwise keep it for a retry.
  const handleRefreshFailure = useCallback(
    (e: unknown) => {
      // The session was already replaced (logout/login) while refreshing: leave the new state alone.
      if (e instanceof SupersededError) return;
      endSession(isTokenRejected(e) || !getRefreshToken());
    },
    [endSession],
  );

  // Layout effects run before child passive effects, so children can already use the API.
  useLayoutEffect(() => {
    setAuthHandlers({
      getAccessToken: () => accessRef.current,
      refresh: async () => {
        const gen = genRef.current;
        const r = await exchangeStored();
        if (gen !== genRef.current) throw new SupersededError();
        apply(r);
        return r.accessToken;
      },
      onAuthFailure: handleRefreshFailure,
    });
  }, [apply, handleRefreshFailure]);

  useEffect(() => {
    if (!getRefreshToken()) return;
    const gen = genRef.current;
    exchangeStored()
      .then((r) => {
        if (gen === genRef.current) apply(r);
      })
      .catch((e: unknown) => {
        if (gen === genRef.current) handleRefreshFailure(e);
      });
  }, [apply, handleRefreshFailure]);

  const value = useMemo<AuthContextValue>(
    () => ({
      status,
      roles: session.roles,
      userId: session.userId,
      userLogin: session.login,
      isAdmin: session.roles.some((r) => r.toLowerCase() === ADMIN_ROLE),
      login: async (l, p) => {
        // Supersede any still-pending startup exchange so its late failure cannot reset this session.
        const gen = ++genRef.current;
        const r = await apiLogin(l, p);
        if (gen === genRef.current) apply(r);
      },
      logout: reset,
    }),
    [status, session, apply, reset],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

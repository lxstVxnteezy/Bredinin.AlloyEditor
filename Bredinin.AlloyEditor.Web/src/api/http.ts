import axios, { isAxiosError, type InternalAxiosRequestConfig } from "axios";
import { toApiError } from "./errors";

export interface AuthHandlers {
  getAccessToken(): string | null;
  refresh(): Promise<string>;
  onAuthFailure(): void;
}

type RetriableConfig = InternalAxiosRequestConfig & { _retried?: boolean };

const AUTH_PATHS = ["/api/gateway/identity/login", "/api/gateway/identity/refresh-token"];

let handlers: AuthHandlers | null = null;
let refreshPromise: Promise<string> | null = null;

export function setAuthHandlers(h: AuthHandlers): void {
  handlers = h;
}

export const http = axios.create({ baseURL: "" });

http.interceptors.request.use((config) => {
  const token = handlers?.getAccessToken();
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

function isAuthRequest(url: string | undefined): boolean {
  return !!url && AUTH_PATHS.some((p) => url.includes(p));
}

function refreshOnce(h: AuthHandlers): Promise<string> {
  refreshPromise ??= h.refresh().finally(() => {
    refreshPromise = null;
  });
  return refreshPromise;
}

http.interceptors.response.use(
  (r) => r,
  async (e: unknown) => {
    const config = isAxiosError(e) ? (e.config as RetriableConfig | undefined) : undefined;
    const h = handlers;
    if (
      h &&
      config &&
      isAxiosError(e) &&
      e.response?.status === 401 &&
      !config._retried &&
      !isAuthRequest(config.url)
    ) {
      config._retried = true;
      let newToken: string;
      try {
        newToken = await refreshOnce(h);
      } catch {
        h.onAuthFailure();
        throw toApiError(e);
      }
      config.headers.Authorization = `Bearer ${newToken}`;
      return http.request(config);
    }
    throw toApiError(e);
  },
);

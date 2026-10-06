import axios from "axios";
import { toApiError } from "./errors";

export interface AuthHandlers {
  getAccessToken(): string | null;
  refresh(): Promise<string>;
  onAuthFailure(): void;
}

let handlers: AuthHandlers | null = null;

export function setAuthHandlers(h: AuthHandlers): void {
  handlers = h;
}

export const http = axios.create({ baseURL: "" });

http.interceptors.request.use((config) => {
  const token = handlers?.getAccessToken();
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

http.interceptors.response.use(
  (r) => r,
  (e) => Promise.reject(toApiError(e)),
);

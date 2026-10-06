import { isAxiosError } from "axios";

export class ApiError extends Error {
  status: number;
  constructor(status: number, message: string) {
    super(message);
    this.name = "ApiError";
    this.status = status;
  }
}

function extractMessage(data: unknown): string | null {
  if (typeof data === "string" && data.trim()) return data;
  if (data && typeof data === "object") {
    const d = data as Record<string, unknown>;
    for (const key of ["message", "title", "detail"]) {
      const v = d[key];
      if (typeof v === "string" && v.trim()) return v;
    }
  }
  return null;
}

export function toApiError(e: unknown): ApiError {
  if (e instanceof ApiError) return e;
  if (isAxiosError(e)) {
    if (!e.response) return new ApiError(0, "Нет связи с сервером");
    const { status, data } = e.response;
    if (status === 429) return new ApiError(status, "Слишком много запросов, повторите позже");
    if (status === 403) return new ApiError(status, "Недостаточно прав");
    return new ApiError(status, extractMessage(data) ?? `Ошибка сервера (${status})`);
  }
  return new ApiError(0, e instanceof Error ? e.message : "Неизвестная ошибка");
}

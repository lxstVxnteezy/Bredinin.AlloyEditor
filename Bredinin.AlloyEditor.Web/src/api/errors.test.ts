import { AxiosError } from "axios";
import { describe, expect, it } from "vitest";
import { ApiError, toApiError } from "./errors";

function axiosErr(status: number, data: unknown): AxiosError {
  const e = new AxiosError("fail");
  e.response = { status, data, statusText: "", headers: {}, config: {} as never };
  return e;
}

describe("toApiError", () => {
  it("takes string body for 400", () => {
    const r = toApiError(axiosErr(400, "Имя занято"));
    expect(r).toBeInstanceOf(ApiError);
    expect(r.status).toBe(400);
    expect(r.message).toBe("Имя занято");
  });
  it("takes message from object body", () => {
    const r = toApiError(axiosErr(500, { message: "x" }));
    expect(r.status).toBe(500);
    expect(r.message).toBe("x");
  });
  it("maps network error", () => {
    const r = toApiError(new AxiosError("Network Error"));
    expect(r.status).toBe(0);
    expect(r.message).toBe("Нет связи с сервером");
  });
  it("maps 429", () => {
    expect(toApiError(axiosErr(429, "")).message).toBe("Слишком много запросов, повторите позже");
  });
  it("maps 403", () => {
    expect(toApiError(axiosErr(403, "")).message).toBe("Недостаточно прав");
  });
});

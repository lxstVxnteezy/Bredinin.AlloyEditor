import { afterEach, beforeEach, describe, expect, it, vi, type Mock } from "vitest";
import { http } from "./http";
import { setAuthHandlers } from "./http";
import { ApiError } from "./errors";
import { http as mswHttp, HttpResponse } from "msw";
import { server } from "../test/server";

const REFRESH = "/api/gateway/identity/refresh-token";
let token: string | null;
let onAuthFailure: Mock<(e?: unknown) => void>;
let refresh: Mock<() => Promise<string>>;

beforeEach(() => {
  token = "old";
  onAuthFailure = vi.fn<(e?: unknown) => void>();
  refresh = vi.fn<() => Promise<string>>(async () => {
    const r = await http.post<{ accessToken: string }>(REFRESH, { refreshToken: "r" });
    token = r.data.accessToken;
    return token;
  });
  setAuthHandlers({ getAccessToken: () => token, refresh, onAuthFailure });
});
afterEach(() => vi.restoreAllMocks());

function dataHandler(counter?: { n: number }) {
  return mswHttp.get("/api/gateway/data", ({ request }) => {
    if (counter) counter.n++;
    return request.headers.get("Authorization") === "Bearer new"
      ? HttpResponse.json({ ok: true })
      : new HttpResponse(null, { status: 401 });
  });
}

describe("refresh interceptor", () => {
  it("(a) refreshes once on 401 and retries with the new token", async () => {
    let refreshCalls = 0;
    server.use(
      dataHandler(),
      mswHttp.post(REFRESH, () => {
        refreshCalls++;
        return HttpResponse.json({ accessToken: "new", refreshToken: "r2" });
      }),
    );
    const r = await http.get("/api/gateway/data");
    expect(r.data).toEqual({ ok: true });
    expect(refreshCalls).toBe(1);
  });

  it("(b) parallel 401s trigger exactly one refresh", async () => {
    let refreshCalls = 0;
    server.use(
      dataHandler(),
      mswHttp.post(REFRESH, async () => {
        refreshCalls++;
        await new Promise((res) => setTimeout(res, 30));
        return HttpResponse.json({ accessToken: "new", refreshToken: "r2" });
      }),
    );
    const rs = await Promise.all([http.get("/api/gateway/data"), http.get("/api/gateway/data")]);
    expect(rs.map((r) => r.status)).toEqual([200, 200]);
    expect(refreshCalls).toBe(1);
  });

  it("(c) failed refresh calls onAuthFailure and rejects", async () => {
    server.use(
      dataHandler(),
      mswHttp.post(REFRESH, () => new HttpResponse(null, { status: 401 })),
    );
    await expect(http.get("/api/gateway/data")).rejects.toBeInstanceOf(ApiError);
    expect(onAuthFailure).toHaveBeenCalledTimes(1);
  });

  it("(c2) transient refresh failure (500) still notifies with the error", async () => {
    server.use(
      dataHandler(),
      mswHttp.post(REFRESH, () => new HttpResponse(null, { status: 500 })),
    );
    await expect(http.get("/api/gateway/data")).rejects.toBeInstanceOf(ApiError);
    expect(onAuthFailure).toHaveBeenCalledTimes(1);
    expect(onAuthFailure.mock.calls[0]![0]).toMatchObject({ status: 500 });
  });

  it("(d) repeated 401 after successful refresh does not loop", async () => {
    const counter = { n: 0 };
    server.use(
      mswHttp.get("/api/gateway/data", () => {
        counter.n++;
        return new HttpResponse(null, { status: 401 });
      }),
      mswHttp.post(REFRESH, () => HttpResponse.json({ accessToken: "new", refreshToken: "r2" })),
    );
    await expect(http.get("/api/gateway/data")).rejects.toMatchObject({ status: 401 });
    expect(counter.n).toBe(2);
    expect(refresh).toHaveBeenCalledTimes(1);
  });

  it("does not refresh on login 401", async () => {
    server.use(mswHttp.post("/api/gateway/identity/login", () => new HttpResponse(null, { status: 401 })));
    await expect(http.post("/api/gateway/identity/login", {})).rejects.toMatchObject({ status: 401 });
    expect(refresh).not.toHaveBeenCalled();
  });
});

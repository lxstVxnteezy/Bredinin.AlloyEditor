import { StrictMode, useEffect } from "react";
import { beforeEach, describe, expect, it } from "vitest";
import { act, render, screen, waitFor } from "@testing-library/react";
import { http as mswHttp, HttpResponse } from "msw";
import { server } from "../test/server";
import { AuthProvider } from "./AuthProvider";
import { useAuth, type AuthContextValue } from "./useAuth";
import { http } from "../api";

function Probe() {
  const { status, isAdmin } = useAuth();
  return <div>{`${status}:${isAdmin}`}</div>;
}

beforeEach(() => localStorage.clear());

describe("AuthProvider", () => {
  it("is anonymous without stored token", async () => {
    render(
      <AuthProvider>
        <Probe />
      </AuthProvider>,
    );
    expect(await screen.findByText("anonymous:false")).toBeInTheDocument();
  });

  it("exchanges stored refresh token", async () => {
    localStorage.setItem("alloy.refreshToken", "r1");
    const b64 = (o: object) => btoa(JSON.stringify(o)).replace(/=+$/, "");
    const role = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
    const access = `${b64({})}.${b64({ [role]: "admin" })}.s`;
    server.use(
      mswHttp.post("/api/gateway/identity/refresh-token", () =>
        HttpResponse.json({ accessToken: access, refreshToken: "r2" }),
      ),
    );
    render(
      <AuthProvider>
        <Probe />
      </AuthProvider>,
    );
    expect(await screen.findByText("authenticated:true")).toBeInTheDocument();
    expect(localStorage.getItem("alloy.refreshToken")).toBe("r2");
  });

  it("becomes anonymous when exchange fails", async () => {
    localStorage.setItem("alloy.refreshToken", "bad");
    server.use(
      mswHttp.post("/api/gateway/identity/refresh-token", () => new HttpResponse(null, { status: 401 })),
    );
    render(
      <AuthProvider>
        <Probe />
      </AuthProvider>,
    );
    await waitFor(() => expect(screen.getByText("anonymous:false")).toBeInTheDocument());
    expect(localStorage.getItem("alloy.refreshToken")).toBeNull();
  });

  describe("single flight and logout", () => {
    const REFRESH = "/api/gateway/identity/refresh-token";
    const holder: { current: AuthContextValue | null } = { current: null };
    const auth = {
      login: (l: string, p: string) => holder.current!.login(l, p),
      logout: () => holder.current!.logout(),
    };
    function Capture() {
      const a = useAuth();
      useEffect(() => {
        holder.current = a;
      });
      return <div>{a.status}</div>;
    }

    function rotating(counter: { n: number }, delayMs = 0) {
      return mswHttp.post(REFRESH, async ({ request }) => {
        counter.n++;
        const body = (await request.json()) as { refreshToken: string };
        if (delayMs) await new Promise((r) => setTimeout(r, delayMs));
        if (body.refreshToken !== "r1") return new HttpResponse(null, { status: 401 });
        return HttpResponse.json({ accessToken: "a.e30.s", refreshToken: "r2" });
      });
    }

    it("StrictMode double mount performs one exchange and stays authenticated", async () => {
      localStorage.setItem("alloy.refreshToken", "r1");
      const counter = { n: 0 };
      server.use(rotating(counter, 20));
      render(
        <StrictMode>
          <AuthProvider>
            <Capture />
          </AuthProvider>
        </StrictMode>,
      );
      expect(await screen.findByText("authenticated")).toBeInTheDocument();
      expect(counter.n).toBe(1);
      expect(localStorage.getItem("alloy.refreshToken")).toBe("r2");
    });

    it("401 refresh racing the mount exchange shares one request", async () => {
      localStorage.setItem("alloy.refreshToken", "r1");
      const counter = { n: 0 };
      server.use(
        rotating(counter, 50),
        mswHttp.get("/api/gateway/data", ({ request }) =>
          request.headers.get("Authorization") === "Bearer a.e30.s"
            ? HttpResponse.json({ ok: true })
            : new HttpResponse(null, { status: 401 }),
        ),
      );
      let result: unknown;
      function Racer() {
        useEffect(() => {
          void http.get("/api/gateway/data").then((r) => (result = r.data));
        }, []);
        return <Capture />;
      }
      render(
        <AuthProvider>
          <Racer />
        </AuthProvider>,
      );
      expect(await screen.findByText("authenticated")).toBeInTheDocument();
      await waitFor(() => expect(result).toEqual({ ok: true }));
      expect(counter.n).toBe(1);
      expect(localStorage.getItem("alloy.refreshToken")).toBe("r2");
    });

    it("logout during pending login leaves user anonymous", async () => {
      server.use(
        mswHttp.post("/api/gateway/identity/login", async () => {
          await new Promise((r) => setTimeout(r, 30));
          return HttpResponse.json({ accessToken: "a.e30.s", refreshToken: "rL" });
        }),
      );
      render(
        <AuthProvider>
          <Capture />
        </AuthProvider>,
      );
      await screen.findByText("anonymous");
      const pending = auth.login("u", "p");
      auth.logout();
      await pending;
      expect(screen.getByText("anonymous")).toBeInTheDocument();
      expect(localStorage.getItem("alloy.refreshToken")).toBeNull();
    });

    it("logout during pending mount refresh leaves user anonymous", async () => {
      localStorage.setItem("alloy.refreshToken", "r1");
      const counter = { n: 0 };
      server.use(rotating(counter, 40));
      render(
        <AuthProvider>
          <Capture />
        </AuthProvider>,
      );
      await waitFor(() => expect(counter.n).toBe(1));
      act(() => auth.logout());
      await new Promise((r) => setTimeout(r, 100));
      expect(screen.getByText("anonymous")).toBeInTheDocument();
      expect(localStorage.getItem("alloy.refreshToken")).toBeNull();
    });
  });
});

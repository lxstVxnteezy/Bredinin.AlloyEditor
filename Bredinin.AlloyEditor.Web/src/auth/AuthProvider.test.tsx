import { beforeEach, describe, expect, it } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import { http as mswHttp, HttpResponse } from "msw";
import { server } from "../test/server";
import { AuthProvider } from "./AuthProvider";
import { useAuth } from "./useAuth";

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
});

import { beforeEach, describe, expect, it } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { http as mswHttp, HttpResponse } from "msw";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { server } from "../../test/server";
import { AuthProvider } from "../../auth/AuthProvider";
import { AuthContext } from "../../auth/useAuth";
import { LoginPage } from "./LoginPage";

const b64 = (o: object) => btoa(JSON.stringify(o)).replace(/=+$/, "");
const access = `${b64({})}.${b64({})}.s`;
const LOGIN_URL = "/api/gateway/identity/login";
const WRONG = "Неверный логин или пароль";

function routes() {
  return (
    <MemoryRouter initialEntries={["/login"]}>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/grades" element={<div>grades page</div>} />
      </Routes>
    </MemoryRouter>
  );
}

function renderLogin() {
  return render(<AuthProvider>{routes()}</AuthProvider>);
}

async function fill(user: ReturnType<typeof userEvent.setup>) {
  await user.type(screen.getByLabelText("Логин"), "bob");
  await user.type(screen.getByLabelText("Пароль"), "secret");
}

beforeEach(() => localStorage.clear());

describe("LoginPage", () => {
  it("navigates to /grades after successful login", async () => {
    let body: unknown;
    server.use(
      mswHttp.post(LOGIN_URL, async ({ request }) => {
        body = await request.json();
        return HttpResponse.json({ accessToken: access, refreshToken: "r1" });
      }),
    );
    const user = userEvent.setup();
    renderLogin();
    await fill(user);
    await user.click(screen.getByRole("button", { name: "Войти" }));
    expect(await screen.findByText("grades page")).toBeInTheDocument();
    expect(body).toEqual({ login: "bob", password: "secret" });
  });

  it.each([401, 500])("shows wrong-credentials message on %i", async (status) => {
    server.use(mswHttp.post(LOGIN_URL, () => new HttpResponse(null, { status })));
    const user = userEvent.setup();
    renderLogin();
    await fill(user);
    await user.click(screen.getByRole("button", { name: "Войти" }));
    expect(await screen.findByText(WRONG)).toBeInTheDocument();
    expect(screen.queryByText("grades page")).not.toBeInTheDocument();
  });

  it("shows the ApiError message for other failures", async () => {
    server.use(mswHttp.post(LOGIN_URL, () => new HttpResponse(null, { status: 429 })));
    const user = userEvent.setup();
    renderLogin();
    await fill(user);
    await user.click(screen.getByRole("button", { name: "Войти" }));
    expect(await screen.findByText("Слишком много запросов, повторите позже")).toBeInTheDocument();
  });

  it("blocks submit with empty fields", async () => {
    let calls = 0;
    server.use(
      mswHttp.post(LOGIN_URL, () => {
        calls++;
        return new HttpResponse(null, { status: 401 });
      }),
    );
    const user = userEvent.setup();
    renderLogin();
    await user.click(screen.getByRole("button", { name: "Войти" }));
    expect(await screen.findAllByText("Обязательное поле")).toHaveLength(2);
    expect(calls).toBe(0);
  });

  it("disables the button while the request is pending", async () => {
    let release!: () => void;
    const gate = new Promise<void>((r) => (release = r));
    server.use(
      mswHttp.post(LOGIN_URL, async () => {
        await gate;
        return new HttpResponse(null, { status: 401 });
      }),
    );
    const user = userEvent.setup();
    renderLogin();
    await fill(user);
    await user.click(screen.getByRole("button", { name: "Войти" }));
    await waitFor(() => expect(screen.getByRole("button", { name: "Войти" })).toBeDisabled());
    release();
    expect(await screen.findByText(WRONG)).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Войти" })).toBeEnabled();
  });

  it("redirects an authenticated user to /grades", () => {
    render(
      <AuthContext.Provider
        value={{
          status: "authenticated",
          roles: [],
          userId: "u1",
          isAdmin: false,
          login: async () => {},
          logout: () => {},
        }}
      >
        {routes()}
      </AuthContext.Provider>,
    );
    expect(screen.getByText("grades page")).toBeInTheDocument();
  });
});

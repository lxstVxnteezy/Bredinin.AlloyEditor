import { describe, expect, it } from "vitest";
import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { NotificationsProvider } from "../components/Notifications";
import { AuthContext, type AuthContextValue } from "./useAuth";
import { AppRoutes } from "../router";

function renderAt(path: string, auth: Partial<AuthContextValue>) {
  const value: AuthContextValue = {
    status: "anonymous",
    roles: [],
    userId: null,
    isAdmin: false,
    login: async () => {},
    logout: () => {},
    ...auth,
  };
  return render(
    <QueryClientProvider client={new QueryClient({ defaultOptions: { queries: { retry: false } } })}>
      <NotificationsProvider>
        <AuthContext.Provider value={value}>
          <MemoryRouter initialEntries={[path]}>
            <AppRoutes />
          </MemoryRouter>
        </AuthContext.Provider>
      </NotificationsProvider>
    </QueryClientProvider>,
  );
}

const user: Partial<AuthContextValue> = { status: "authenticated", roles: ["user"], userId: "u1" };
const admin: Partial<AuthContextValue> = { status: "authenticated", roles: ["admin"], userId: "a1", isAdmin: true };

describe("route guards", () => {
  it("redirects anonymous user from /grades to /login", () => {
    renderAt("/grades", {});
    expect(screen.getByRole("heading", { name: "Вход" })).toBeInTheDocument();
  });

  it("shows a spinner while loading", () => {
    renderAt("/grades", { status: "loading" });
    expect(screen.getByRole("progressbar")).toBeInTheDocument();
  });

  it("redirects / to /grades for authenticated user", () => {
    renderAt("/", user);
    expect(screen.getByRole("heading", { name: "Марки" })).toBeInTheDocument();
  });

  it("denies non-admin on /users", () => {
    renderAt("/users", user);
    expect(screen.getByText("Недостаточно прав")).toBeInTheDocument();
  });

  it("shows the login in the header instead of the raw id", () => {
    renderAt("/grades", { ...user, userLogin: "ivan" });
    expect(screen.getByText("ivan")).toBeInTheDocument();
    expect(screen.getByText("пользователь")).toBeInTheDocument();
    expect(screen.queryByText(/u1/)).not.toBeInTheDocument();
  });

  it("hides the Users menu item from non-admin", () => {
    renderAt("/grades", user);
    expect(screen.getByRole("link", { name: "Марки" })).toBeInTheDocument();
    expect(screen.queryByRole("link", { name: "Пользователи" })).not.toBeInTheDocument();
  });

  it("lets admin open /users and see the menu item", () => {
    renderAt("/users", admin);
    expect(screen.getByRole("heading", { name: "Пользователи" })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Пользователи" })).toBeInTheDocument();
  });
});

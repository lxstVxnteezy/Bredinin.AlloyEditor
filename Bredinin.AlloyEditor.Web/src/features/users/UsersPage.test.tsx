import { describe, expect, it } from "vitest";
import { fireEvent, screen, waitFor, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { http, HttpResponse } from "msw";
import { server } from "../../test/server";
import { renderWithProviders } from "../../test/renderWithProviders";
import { NotificationsProvider } from "../../components/Notifications";
import { AuthContext, type AuthContextValue } from "../../auth/useAuth";
import { UsersPage } from "./UsersPage";

const ADMIN = "1798a758-829b-4839-9180-0a205545a450";
const USER = "7a593027-99d0-4a47-91b1-5c73049cecc1";

const admin = { id: "u1", login: "admin", firstName: "Иван", lastName: "Петров", secondName: "Сергеевич", age: 40, roleIds: [ADMIN] };
const bob = { id: "u2", login: "bob", firstName: "Боб", lastName: "Смит", secondName: "Джонович", age: 30, roleIds: [USER] };

const renderPage = (userId: string | null = null) => {
  const auth: AuthContextValue = {
    status: "authenticated", roles: ["admin"], userId, isAdmin: true, login: async () => {}, logout: () => {},
  };
  return renderWithProviders(
    <NotificationsProvider>
      <AuthContext.Provider value={auth}>
        <UsersPage />
      </AuthContext.Provider>
    </NotificationsProvider>,
  );
};

const rowOf = async (login: string) => (await screen.findByText(login)).closest("tr") as HTMLElement;

describe("UsersPage", () => {
  it("lists users with full name and role names", async () => {
    server.use(http.get("/api/gateway/admin/search-users", () => HttpResponse.json([admin, bob])));
    renderPage();
    const row = await rowOf("admin");
    expect(within(row).getByText("Петров Иван Сергеевич")).toBeInTheDocument();
    expect(within(row).getByText("40")).toBeInTheDocument();
    expect(within(row).getByText("Администратор")).toBeInTheDocument();
    expect(within(await rowOf("bob")).getByText("Пользователь")).toBeInTheDocument();
  });

  it("shows error and empty states", async () => {
    server.use(http.get("/api/gateway/admin/search-users", () => HttpResponse.json({ message: "boom" }, { status: 500 })));
    const { unmount } = renderPage();
    expect(screen.getByLabelText("Загрузка")).toBeInTheDocument();
    expect(await screen.findByRole("alert")).toBeInTheDocument();
    unmount();
    server.use(http.get("/api/gateway/admin/search-users", () => HttpResponse.json([])));
    renderPage();
    expect(await screen.findByText("Пользователей нет")).toBeInTheDocument();
  });

  it("filters by search and shows no-match state", async () => {
    server.use(http.get("/api/gateway/admin/search-users", () => HttpResponse.json([admin, bob])));
    const user = userEvent.setup();
    renderPage();
    await rowOf("admin");
    await user.type(screen.getByLabelText("Поиск"), "смит");
    expect(screen.queryByText("admin")).not.toBeInTheDocument();
    expect(screen.getByText("bob")).toBeInTheDocument();
    await user.clear(screen.getByLabelText("Поиск"));
    await user.type(screen.getByLabelText("Поиск"), "zzz");
    expect(screen.getByText("Ничего не найдено")).toBeInTheDocument();
  });

  it("creates a user with the right body and refreshes the table", async () => {
    let users = [admin];
    let body: unknown;
    server.use(
      http.get("/api/gateway/admin/search-users", () => HttpResponse.json(users)),
      http.post("/api/gateway/admin/user", async ({ request }) => {
        body = await request.json();
        users = [admin, bob];
        return HttpResponse.json("u2");
      }),
    );
    const user = userEvent.setup();
    renderPage();
    await rowOf("admin");
    await user.click(screen.getByRole("button", { name: "Создать" }));
    const dialog = screen.getByRole("dialog");
    expect(within(dialog).getByLabelText(/Пароль/)).toHaveAttribute("type", "password");
    await user.type(within(dialog).getByLabelText(/Логин/), "bob");
    await user.type(within(dialog).getByLabelText(/Фамилия/), "Смит");
    await user.type(within(dialog).getByLabelText(/^Имя/), "Боб");
    await user.type(within(dialog).getByLabelText(/Отчество/), "Джонович");
    await user.type(within(dialog).getByLabelText(/Возраст/), "30");
    await user.type(within(dialog).getByLabelText(/Пароль/), "Secret123!");
    await user.click(within(dialog).getByRole("checkbox", { name: "Пользователь" }));
    await user.click(within(dialog).getByRole("button", { name: "Создать" }));
    expect(await screen.findByText("bob")).toBeInTheDocument();
    expect(body).toEqual({
      login: "bob", firstName: "Боб", lastName: "Смит", secondName: "Джонович",
      age: 30, password: "Secret123!", roleIds: [USER],
    });
    await waitFor(() => expect(screen.queryByRole("dialog")).not.toBeInTheDocument());
    expect(screen.getByText("Пользователь создан")).toBeInTheDocument();
  });

  it("keeps the dialog and shows the server error on create failure", async () => {
    server.use(
      http.get("/api/gateway/admin/search-users", () => HttpResponse.json([admin])),
      http.post("/api/gateway/admin/user", () => HttpResponse.json({ message: "Логин занят" }, { status: 409 })),
    );
    const user = userEvent.setup();
    renderPage();
    await rowOf("admin");
    await user.click(screen.getByRole("button", { name: "Создать" }));
    const dialog = screen.getByRole("dialog");
    await user.type(within(dialog).getByLabelText(/Логин/), "admin");
    await user.type(within(dialog).getByLabelText(/Фамилия/), "А");
    await user.type(within(dialog).getByLabelText(/^Имя/), "Б");
    await user.type(within(dialog).getByLabelText(/Отчество/), "В");
    await user.type(within(dialog).getByLabelText(/Возраст/), "20");
    await user.type(within(dialog).getByLabelText(/Пароль/), "pw");
    await user.click(within(dialog).getByRole("button", { name: "Создать" }));
    expect(await within(dialog).findByText("Логин занят")).toBeInTheDocument();
  });

  it("validates required fields before sending", async () => {
    let posted = 0;
    server.use(
      http.get("/api/gateway/admin/search-users", () => HttpResponse.json([admin])),
      http.post("/api/gateway/admin/user", () => {
        posted++;
        return HttpResponse.json("x");
      }),
    );
    const user = userEvent.setup();
    renderPage();
    await rowOf("admin");
    await user.click(screen.getByRole("button", { name: "Создать" }));
    const dialog = screen.getByRole("dialog");
    await user.click(within(dialog).getByRole("button", { name: "Создать" }));
    expect(within(dialog).getByText("Укажите логин")).toBeInTheDocument();
    expect(posted).toBe(0);
  });

  it("edits a user with the right body and refreshes the table", async () => {
    let users = [admin, bob];
    let body: unknown;
    server.use(
      http.get("/api/gateway/admin/search-users", () => HttpResponse.json(users)),
      http.put("/api/gateway/admin/user", async ({ request }) => {
        body = await request.json();
        users = [admin, { ...bob, firstName: "Роберт" }];
        return HttpResponse.json({});
      }),
    );
    const user = userEvent.setup();
    renderPage();
    await user.click(within(await rowOf("bob")).getByRole("button", { name: "Изменить" }));
    const dialog = screen.getByRole("dialog");
    expect(within(dialog).queryByLabelText(/Пароль/)).not.toBeInTheDocument();
    expect(within(dialog).getByLabelText(/Логин/)).toBeDisabled();
    const first = within(dialog).getByLabelText(/^Имя/);
    await user.clear(first);
    await user.type(first, "Роберт");
    await user.click(within(dialog).getByRole("checkbox", { name: "Администратор" }));
    await user.click(within(dialog).getByRole("button", { name: "Сохранить" }));
    expect(await screen.findByText("Смит Роберт Джонович")).toBeInTheDocument();
    expect(body).toEqual({
      id: "u2", firstName: "Роберт", lastName: "Смит", secondName: "Джонович",
      age: 30, roleIds: [ADMIN, USER],
    });
  });

  it("deletes only after confirmation and refreshes", async () => {
    let users = [admin, bob];
    let deleted = 0;
    server.use(
      http.get("/api/gateway/admin/search-users", () => HttpResponse.json(users)),
      http.delete("/api/gateway/admin/u2", () => {
        deleted++;
        users = [admin];
        return new HttpResponse(null, { status: 204 });
      }),
    );
    const user = userEvent.setup();
    renderPage();
    await user.click(within(await rowOf("bob")).getByRole("button", { name: "Удалить" }));
    await user.click(screen.getByRole("button", { name: "Отмена" }));
    await waitFor(() => expect(screen.queryByRole("dialog")).not.toBeInTheDocument());
    expect(deleted).toBe(0);
    await user.click(within(await rowOf("bob")).getByRole("button", { name: "Удалить" }));
    await user.click(screen.getByRole("button", { name: "Подтвердить" }));
    await waitFor(() => expect(screen.queryByText("bob")).not.toBeInTheDocument());
    expect(deleted).toBe(1);
    expect(await screen.findByText("Пользователь удалён")).toBeInTheDocument();
  });

  it("keeps the row and notifies when delete fails", async () => {
    server.use(
      http.get("/api/gateway/admin/search-users", () => HttpResponse.json([bob])),
      http.delete("/api/gateway/admin/u2", () => HttpResponse.json({ message: "Нельзя" }, { status: 409 })),
    );
    const user = userEvent.setup();
    renderPage();
    await user.click(within(await rowOf("bob")).getByRole("button", { name: "Удалить" }));
    await user.click(screen.getByRole("button", { name: "Подтвердить" }));
    expect(await screen.findByText("Нельзя")).toBeInTheDocument();
    expect(screen.getByText("bob")).toBeInTheDocument();
  });

  it("revokes access only after confirmation", async () => {
    let revoked = 0;
    server.use(
      http.get("/api/gateway/admin/search-users", () => HttpResponse.json([bob])),
      http.post("/api/gateway/identity/u2/revoke-access", () => {
        revoked++;
        return new HttpResponse(null, { status: 204 });
      }),
    );
    const user = userEvent.setup();
    renderPage();
    await user.click(within(await rowOf("bob")).getByRole("button", { name: "Отозвать доступ" }));
    await user.click(screen.getByRole("button", { name: "Отмена" }));
    await waitFor(() => expect(screen.queryByRole("dialog")).not.toBeInTheDocument());
    expect(revoked).toBe(0);
    await user.click(within(await rowOf("bob")).getByRole("button", { name: "Отозвать доступ" }));
    await user.click(screen.getByRole("button", { name: "Подтвердить" }));
    await waitFor(() => expect(revoked).toBe(1));
    expect(await screen.findByText("Доступ отозван")).toBeInTheDocument();
  });

  it("renders a disabled reset-password button", async () => {
    server.use(http.get("/api/gateway/admin/search-users", () => HttpResponse.json([bob])));
    renderPage();
    expect(within(await rowOf("bob")).getByRole("button", { name: "Сбросить пароль" })).toBeDisabled();
  });

  it("disables delete and revoke on the current user's own row only", async () => {
    let requests = 0;
    server.use(
      http.get("/api/gateway/admin/search-users", () => HttpResponse.json([admin, bob])),
      http.delete("/api/gateway/admin/:id", () => {
        requests++;
        return new HttpResponse(null, { status: 204 });
      }),
      http.post("/api/gateway/identity/:id/revoke-access", () => {
        requests++;
        return new HttpResponse(null, { status: 204 });
      }),
    );
    renderPage("U1");
    const own = await rowOf("admin");
    const other = await rowOf("bob");
    expect(within(own).getByRole("button", { name: "Удалить" })).toBeDisabled();
    expect(within(own).getByRole("button", { name: "Отозвать доступ" })).toBeDisabled();
    expect(within(own).getByRole("button", { name: "Изменить" })).toBeEnabled();
    expect(within(other).getByRole("button", { name: "Удалить" })).toBeEnabled();
    expect(within(other).getByRole("button", { name: "Отозвать доступ" })).toBeEnabled();
    fireEvent.click(within(own).getByRole("button", { name: "Удалить" }));
    fireEvent.click(within(own).getByRole("button", { name: "Отозвать доступ" }));
    expect(screen.queryByRole("dialog")).not.toBeInTheDocument();
    expect(requests).toBe(0);
  });

  it("disables the admin role checkbox when editing your own account", async () => {
    server.use(http.get("/api/gateway/admin/search-users", () => HttpResponse.json([admin, bob])));
    const user = userEvent.setup();
    renderPage("U1");
    await user.click(within(await rowOf("admin")).getByRole("button", { name: "Изменить" }));
    expect(within(screen.getByRole("dialog")).getByRole("checkbox", { name: "Администратор" })).toBeDisabled();
    await user.click(within(screen.getByRole("dialog")).getByRole("button", { name: "Отмена" }));
    await user.click(within(await rowOf("bob")).getByRole("button", { name: "Изменить" }));
    expect(within(screen.getByRole("dialog")).getByRole("checkbox", { name: "Администратор" })).toBeEnabled();
  });

  it("does not disable anything when the current user id is unknown", async () => {
    server.use(http.get("/api/gateway/admin/search-users", () => HttpResponse.json([admin])));
    renderPage(null);
    const row = await rowOf("admin");
    expect(within(row).getByRole("button", { name: "Удалить" })).toBeEnabled();
    expect(within(row).getByRole("button", { name: "Отозвать доступ" })).toBeEnabled();
  });

  it("keeps the row and notifies when revoke fails", async () => {
    server.use(
      http.get("/api/gateway/admin/search-users", () => HttpResponse.json([bob])),
      http.post("/api/gateway/identity/u2/revoke-access", () =>
        HttpResponse.json({ message: "Сбой отзыва" }, { status: 500 }),
      ),
    );
    const user = userEvent.setup();
    renderPage();
    await user.click(within(await rowOf("bob")).getByRole("button", { name: "Отозвать доступ" }));
    await user.click(screen.getByRole("button", { name: "Подтвердить" }));
    expect(await screen.findByText("Сбой отзыва")).toBeInTheDocument();
    expect(screen.getByText("bob")).toBeInTheDocument();
  });
});

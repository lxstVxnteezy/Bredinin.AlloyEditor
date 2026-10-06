import { describe, expect, it } from "vitest";
import { screen, waitFor, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { http, HttpResponse } from "msw";
import { server } from "../../test/server";
import { renderWithProviders } from "../../test/renderWithProviders";
import { NotificationsProvider } from "../../components/Notifications";
import { GradesPage } from "./GradesPage";

const systems = [{ id: "s1", baseElementId: "e1", name: "Стали" }];
const el = (id: string, name: string, symbol: string) => ({
  id, name, symbol, isBaseForAlloySystem: id === "e1", atomicNumber: 1, atomicWeight: 1, group: 1, period: 1, density: 1,
});
const elements = [el("e1", "Железо", "Fe"), el("e2", "Углерод", "C")];

function setup() {
  let grades: unknown[] = [];
  const posts: unknown[] = [];
  server.use(
    http.get("/api/gateway/alloy-grade", () => HttpResponse.json(grades)),
    http.get("/api/gateway/alloy-system", () => HttpResponse.json(systems)),
    http.get("/api/gateway/dictionary/chemical-elements", () => HttpResponse.json(elements)),
    http.post("/api/gateway/alloy-grade", async ({ request }) => {
      const body = (await request.json()) as { name: string };
      posts.push(body);
      grades = [
        { id: "g9", name: body.name, alloySystemId: "s1", chemicalCompositions: [], heatTreatments: [], mechanicalProperties: [] },
      ];
      return HttpResponse.json("g9");
    }),
  );
  return { posts };
}

async function openDialog() {
  const user = userEvent.setup();
  renderWithProviders(
    <NotificationsProvider>
      <GradesPage />
    </NotificationsProvider>,
  );
  await user.click(await screen.findByRole("button", { name: "Создать" }));
  const dialog = await screen.findByRole("dialog");
  return { user, dialog };
}

async function pick(user: ReturnType<typeof userEvent.setup>, field: HTMLElement, option: string | RegExp) {
  await user.click(field);
  await user.click(await screen.findByRole("option", { name: option }));
}

describe("CreateGradeDialog", () => {
  it("blocks submit with invalid composition and shows errors", async () => {
    const { posts } = setup();
    const { user, dialog } = await openDialog();
    await user.type(within(dialog).getByLabelText(/Название/), "Сталь 20");
    await pick(user, within(dialog).getByLabelText(/Система сплава/), "Стали");
    // base element row is pre-added; leave its value empty and add a duplicate Fe
    await user.click(within(dialog).getByRole("button", { name: "Добавить элемент" }));
    const selects = within(dialog).getAllByLabelText("Элемент");
    await pick(user, selects[1], /Fe/);
    await user.type(within(dialog).getAllByLabelText("Значение")[1], "120");
    await user.click(within(dialog).getByRole("button", { name: "Создать" }));

    const alert = await within(dialog).findByRole("alert");
    expect(within(alert).getByText(/повторяться/)).toBeInTheDocument();
    expect(within(alert).getByText(/Строка 1: укажите значение/)).toBeInTheDocument();
    expect(within(alert).getByText(/не должна превышать 100/)).toBeInTheDocument();
    expect(posts).toHaveLength(0);
    expect(screen.getByRole("dialog")).toBeInTheDocument();
  });

  it("requires a name", async () => {
    const { posts } = setup();
    const { user, dialog } = await openDialog();
    await user.type(within(dialog).getByLabelText(/Название/), "   ");
    await user.click(within(dialog).getByRole("button", { name: "Создать" }));
    expect(await within(dialog).findByText("Укажите название марки")).toBeInTheDocument();
    expect(posts).toHaveLength(0);
  });

  it("posts the exact body, closes, and refreshes the list", async () => {
    const { posts } = setup();
    const { user, dialog } = await openDialog();
    await user.type(within(dialog).getByLabelText(/Название/), "  Сталь 20 ");
    await pick(user, within(dialog).getByLabelText(/Система сплава/), "Стали");
    await user.type(within(dialog).getByLabelText("Значение"), "99");
    await user.click(within(dialog).getByRole("button", { name: "Добавить элемент" }));
    await pick(user, within(dialog).getAllByLabelText("Элемент")[1], /C —/);
    await user.click(within(dialog).getAllByRole("button", { name: "Диапазон" })[1]);
    await user.type(within(dialog).getByLabelText("Мин"), "0,1");
    await user.type(within(dialog).getByLabelText("Макс"), "0.5");
    await user.click(within(dialog).getByRole("button", { name: "Создать" }));

    await waitFor(() => expect(screen.queryByRole("dialog")).not.toBeInTheDocument());
    expect(posts).toEqual([
      {
        name: "Сталь 20",
        alloySystemId: "s1",
        chemicalCompositions: [
          { chemicalElementId: "e1", exactValue: 99 },
          { chemicalElementId: "e2", minValue: 0.1, maxValue: 0.5 },
        ],
        heatTreatments: [],
      },
    ]);
    expect(await screen.findByText("Сталь 20")).toBeInTheDocument();
    expect(await screen.findByText("Марка создана")).toBeInTheDocument();
  });

  it("shows the server error text and keeps the dialog open", async () => {
    setup();
    server.use(http.post("/api/gateway/alloy-grade", () => HttpResponse.text("Имя занято", { status: 400 })));
    const { user, dialog } = await openDialog();
    await user.type(within(dialog).getByLabelText(/Название/), "Сталь 20");
    await pick(user, within(dialog).getByLabelText(/Система сплава/), "Стали");
    await user.type(within(dialog).getByLabelText("Значение"), "100");
    await user.click(within(dialog).getByRole("button", { name: "Создать" }));

    expect(await within(dialog).findByText("Имя занято")).toBeInTheDocument();
    expect(screen.getByRole("dialog")).toBeInTheDocument();
    expect(within(dialog).getByRole("button", { name: "Создать" })).toBeEnabled();
  });
});

import { describe, expect, it } from "vitest";
import { screen, waitFor, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { http, HttpResponse } from "msw";
import { server } from "../../test/server";
import { pagedGrades } from "../../test/paged";
import { renderWithProviders } from "../../test/renderWithProviders";
import { NotificationsProvider } from "../../components/Notifications";
import { GradesPage } from "./GradesPage";

const systems = [
  { id: "s1", baseElementId: "e1", name: "Стали" },
  { id: "s2", baseElementId: "e2", name: "Алюминиевые" },
];
const elements = [
  { id: "e1", name: "Железо", symbol: "Fe", isBaseForAlloySystem: true, atomicNumber: 26, atomicWeight: 55, group: 8, period: 4, density: 7.8 },
  { id: "e2", name: "Углерод", symbol: "C", isBaseForAlloySystem: false, atomicNumber: 6, atomicWeight: 12, group: 14, period: 2, density: 2 },
];
const grade45 = {
  id: "g1", name: "Сталь 45", description: "конструкционная", alloySystemId: "s1",
  chemicalCompositions: [
    { chemicalElementId: "e2", minValue: 0.42, maxValue: 0.5, exactValue: null },
    { chemicalElementId: "unknown-el", minValue: null, maxValue: null, exactValue: 1 },
  ],
  heatTreatments: [], mechanicalProperties: [],
};
const gradeAl = {
  id: "g2", name: "АД31", description: null, alloySystemId: "s2",
  chemicalCompositions: [],
  heatTreatments: [
    { id: "h1", heatTreatmentTypeId: "t1", heatTreatmentTypeName: "Закалка", temperatureExact: 520, isDefault: true },
  ],
  mechanicalProperties: [
    { id: "m1", propertyTypeId: "p1", propertyTypeName: "Твёрдость", propertyTypeUnit: "HB", valueExact: 95 },
  ],
};

function setup(grades: unknown[] = [grade45, gradeAl]) {
  server.use(
    http.get("/api/gateway/alloy-grade", ({ request }) => pagedGrades(grades as { name: string }[], request)),
    http.get("/api/gateway/alloy-system", () => HttpResponse.json(systems)),
    http.get("/api/gateway/dictionary/chemical-elements", () => HttpResponse.json(elements)),
  );
}

const renderPage = () =>
  renderWithProviders(
    <NotificationsProvider>
      <GradesPage />
    </NotificationsProvider>,
  );

describe("GradesPage", () => {
  it("shows grades with resolved system name and element count", async () => {
    setup();
    renderPage();
    const row = (await screen.findByText("Сталь 45")).closest("tr") as HTMLElement;
    await waitFor(() => expect(within(row).getByText("Стали")).toBeInTheDocument());
    expect(within(row).getByText("конструкционная")).toBeInTheDocument();
    expect(within(row).getByText("2")).toBeInTheDocument();
  });

  it("falls back to system id when unknown", async () => {
    setup([{ ...grade45, alloySystemId: "zzz" }]);
    renderPage();
    expect(await screen.findByText("zzz")).toBeInTheDocument();
  });

  it("shows loading, error and empty states", async () => {
    server.use(
      http.get("/api/gateway/alloy-grade", () => HttpResponse.json({ message: "boom" }, { status: 500 })),
      http.get("/api/gateway/alloy-system", () => HttpResponse.json(systems)),
      http.get("/api/gateway/dictionary/chemical-elements", () => HttpResponse.json(elements)),
    );
    const { unmount } = renderPage();
    expect(screen.getByLabelText("Загрузка")).toBeInTheDocument();
    expect(await screen.findByRole("alert")).toBeInTheDocument();
    unmount();

    setup([]);
    renderPage();
    expect(await screen.findByText("Марок пока нет")).toBeInTheDocument();
  });

  it("searches on the server with the trimmed text and shows no-match", async () => {
    setup();
    const searches: (string | null)[] = [];
    server.use(
      http.get("/api/gateway/alloy-grade", ({ request }) => {
        searches.push(new URL(request.url).searchParams.get("search"));
        return pagedGrades([grade45, gradeAl], request);
      }),
    );
    const user = userEvent.setup();
    renderPage();
    await screen.findByText("Сталь 45");
    await user.type(screen.getByLabelText("Поиск по названию"), "  сталь ");
    await waitFor(() => expect(screen.queryByText("АД31")).not.toBeInTheDocument());
    expect(screen.getByText("Сталь 45")).toBeInTheDocument();
    expect(searches).toContain("сталь");
    expect(searches.filter((s) => s !== null && s !== "сталь" && s.includes(" "))).toEqual([]);
    await user.clear(screen.getByLabelText("Поиск по названию"));
    await user.type(screen.getByLabelText("Поиск по названию"), "нет такой");
    expect(await screen.findByText("Ничего не найдено")).toBeInTheDocument();
  });

  it("keeps the chosen page when the unchanged search box settles", async () => {
    const many = Array.from({ length: 25 }, (_, i) => ({
      ...grade45,
      id: `m${i + 1}`,
      name: `Марка ${String(i + 1).padStart(2, "0")}`,
    }));
    setup(many);
    const user = userEvent.setup();
    renderPage();
    await screen.findByText("Марка 01");
    await user.click(screen.getByRole("button", { name: "Следующая страница" }));
    expect(await screen.findByText("Марка 21")).toBeInTheDocument();
    // Longer than the search debounce: the pending timer from mount must not reset the page.
    await new Promise((resolve) => setTimeout(resolve, 500));
    expect(screen.getByText("Марка 21")).toBeInTheDocument();
    expect(screen.getByText("21–25 из 25")).toBeInTheDocument();
  });

  it("pages through the list and resets to the first page on a new search", async () => {
    const many = Array.from({ length: 25 }, (_, i) => ({
      ...grade45,
      id: `m${i + 1}`,
      name: `Марка ${String(i + 1).padStart(2, "0")}`,
    }));
    setup(many);
    const user = userEvent.setup();
    renderPage();
    expect(await screen.findByText("Марка 01")).toBeInTheDocument();
    expect(screen.getByText("Марка 20")).toBeInTheDocument();
    expect(screen.queryByText("Марка 21")).not.toBeInTheDocument();
    expect(screen.getByText("1–20 из 25")).toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: "Следующая страница" }));
    expect(await screen.findByText("Марка 21")).toBeInTheDocument();
    expect(screen.queryByText("Марка 01")).not.toBeInTheDocument();
    expect(screen.getByText("21–25 из 25")).toBeInTheDocument();
    await user.type(screen.getByLabelText("Поиск по названию"), "марка 0");
    // The search is debounced, so give the request more time than the default under a loaded test run.
    expect(await screen.findByText("Марка 01", {}, { timeout: 4000 })).toBeInTheDocument();
    expect(await screen.findByText("1–9 из 9", {}, { timeout: 4000 })).toBeInTheDocument();
  });

  it("filters by alloy system", async () => {
    setup();
    const user = userEvent.setup();
    renderPage();
    await screen.findByText("Сталь 45");
    await user.click(screen.getByLabelText("Система сплава"));
    await user.click(await screen.findByRole("option", { name: "Алюминиевые" }));
    expect(screen.getByText("АД31")).toBeInTheDocument();
    expect(screen.queryByText("Сталь 45")).not.toBeInTheDocument();
    await user.click(screen.getByLabelText("Система сплава"));
    await user.click(await screen.findByRole("option", { name: "Все системы" }));
    expect(screen.getByText("Сталь 45")).toBeInTheDocument();
  });

  it("opens card on click with composition table", async () => {
    setup();
    const user = userEvent.setup();
    renderPage();
    await user.click(await screen.findByText("Сталь 45"));
    const dialog = await screen.findByRole("dialog");
    await waitFor(() => expect(within(dialog).getByText("C")).toBeInTheDocument());
    expect(within(dialog).getByText("0.42")).toBeInTheDocument();
    expect(within(dialog).getByText("0.5")).toBeInTheDocument();
    expect(within(dialog).getByText("unknown-el")).toBeInTheDocument();
    expect(within(dialog).getAllByText("—").length).toBeGreaterThan(0);
    expect(within(dialog).queryByText("Термообработка")).not.toBeInTheDocument();
    expect(within(dialog).queryByText("Механические свойства")).not.toBeInTheDocument();
  });

  it("opens card via keyboard and shows heat treatments and properties", async () => {
    setup();
    const user = userEvent.setup();
    renderPage();
    const row = (await screen.findByText("АД31")).closest("tr") as HTMLElement;
    expect(row).toHaveAttribute("tabindex", "0");
    row.focus();
    await user.keyboard("{Enter}");
    const dialog = await screen.findByRole("dialog");
    expect(within(dialog).getByText("Термообработка")).toBeInTheDocument();
    expect(within(dialog).getByText("Закалка")).toBeInTheDocument();
    expect(within(dialog).getByText("Механические свойства")).toBeInTheDocument();
    expect(within(dialog).getByText("Твёрдость")).toBeInTheDocument();
  });

  it("has disabled edit button and an enabled create button", async () => {
    setup();
    renderPage();
    await screen.findByText("Сталь 45");
    expect(screen.getByRole("button", { name: "Редактировать" })).toBeDisabled();
    expect(screen.getByRole("button", { name: "Создать" })).toBeEnabled();
  });

  it("deletes after confirmation and refetches list", async () => {
    let grades = [grade45, gradeAl];
    let deleted = 0;
    setup();
    server.use(
      http.get("/api/gateway/alloy-grade", ({ request }) => pagedGrades(grades, request)),
      http.delete("/api/gateway/alloy-grade/g1", () => {
        deleted++;
        grades = [gradeAl];
        return new HttpResponse(null, { status: 204 });
      }),
    );
    const user = userEvent.setup();
    renderPage();
    const row = (await screen.findByText("Сталь 45")).closest("tr") as HTMLElement;
    await user.click(within(row).getByRole("button", { name: "Удалить" }));
    expect(deleted).toBe(0);
    expect(screen.getByRole("dialog")).toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: "Подтвердить" }));
    await waitFor(() => expect(screen.queryByText("Сталь 45")).not.toBeInTheDocument());
    expect(deleted).toBe(1);
    expect(screen.getByText("АД31")).toBeInTheDocument();
    expect(await screen.findByText("Марка удалена")).toBeInTheDocument();
  });

  it("steps back a page when the last item of the last page is deleted", async () => {
    let many = Array.from({ length: 21 }, (_, i) => ({
      ...grade45,
      id: `m${i + 1}`,
      name: `Марка ${String(i + 1).padStart(2, "0")}`,
    }));
    setup();
    server.use(
      http.get("/api/gateway/alloy-grade", ({ request }) => pagedGrades(many, request)),
      http.delete("/api/gateway/alloy-grade/m21", () => {
        many = many.filter((g) => g.id !== "m21");
        return new HttpResponse(null, { status: 204 });
      }),
    );
    const user = userEvent.setup();
    renderPage();
    await screen.findByText("Марка 01");
    await user.click(screen.getByRole("button", { name: "Следующая страница" }));
    const row = (await screen.findByText("Марка 21")).closest("tr") as HTMLElement;
    await user.click(within(row).getByRole("button", { name: "Удалить" }));
    await user.click(screen.getByRole("button", { name: "Подтвердить" }));
    expect(await screen.findByText("Марка 20")).toBeInTheDocument();
    expect(screen.getByText("1–20 из 20")).toBeInTheDocument();
  });

  it("does not delete on cancel", async () => {
    setup();
    let deleted = 0;
    server.use(
      http.delete("/api/gateway/alloy-grade/g1", () => {
        deleted++;
        return new HttpResponse(null, { status: 204 });
      }),
    );
    const user = userEvent.setup();
    renderPage();
    const row = (await screen.findByText("Сталь 45")).closest("tr") as HTMLElement;
    await user.click(within(row).getByRole("button", { name: "Удалить" }));
    await user.click(screen.getByRole("button", { name: "Отмена" }));
    await waitFor(() => expect(screen.queryByRole("dialog")).not.toBeInTheDocument());
    expect(deleted).toBe(0);
    expect(screen.getByText("Сталь 45")).toBeInTheDocument();
  });

  it("keeps row and notifies on delete failure; delete click does not open card", async () => {
    setup();
    server.use(
      http.delete("/api/gateway/alloy-grade/g1", () =>
        HttpResponse.json({ message: "Нельзя удалить" }, { status: 409 }),
      ),
    );
    const user = userEvent.setup();
    renderPage();
    const row = (await screen.findByText("Сталь 45")).closest("tr") as HTMLElement;
    await user.click(within(row).getByRole("button", { name: "Удалить" }));
    expect(screen.queryByText("Состав")).not.toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: "Подтвердить" }));
    expect(await screen.findByText("Нельзя удалить")).toBeInTheDocument();
    expect(screen.getByText("Сталь 45")).toBeInTheDocument();
  });
});

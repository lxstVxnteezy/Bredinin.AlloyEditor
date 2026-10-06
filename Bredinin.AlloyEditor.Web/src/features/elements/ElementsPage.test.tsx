import { describe, expect, it } from "vitest";
import { screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { http, HttpResponse } from "msw";
import { server } from "../../test/server";
import { renderWithProviders } from "../../test/renderWithProviders";
import { ElementsPage } from "./ElementsPage";

const URL = "/api/gateway/dictionary/chemical-elements";
const el = (o: object) => ({
  id: "i", name: "N", symbol: "S", isBaseForAlloySystem: false,
  atomicNumber: 1, atomicWeight: 1, group: 1, period: 1, density: 1, ...o,
});
const data = [
  el({ id: "1", name: "Железо", symbol: "Fe", isBaseForAlloySystem: true }),
  el({ id: "2", name: "Углерод", symbol: "C" }),
];

describe("ElementsPage", () => {
  it("shows symbol and name and base badge", async () => {
    server.use(http.get(URL, () => HttpResponse.json(data)));
    renderWithProviders(<ElementsPage />);
    expect(await screen.findByText("Железо")).toBeInTheDocument();
    expect(screen.getByText("Fe")).toBeInTheDocument();
    expect(screen.getAllByText("Базовый")).toHaveLength(1);
  });

  it("filters by search text", async () => {
    server.use(http.get(URL, () => HttpResponse.json(data)));
    const user = userEvent.setup();
    renderWithProviders(<ElementsPage />);
    await screen.findByText("Железо");
    await user.type(screen.getByLabelText("Поиск"), "УГЛ");
    const table = screen.getByRole("table");
    expect(within(table).queryByText("Железо")).not.toBeInTheDocument();
    expect(within(table).getByText("Углерод")).toBeInTheDocument();
  });

  it("shows no-match state for search without results", async () => {
    server.use(http.get(URL, () => HttpResponse.json(data)));
    const user = userEvent.setup();
    renderWithProviders(<ElementsPage />);
    await screen.findByText("Железо");
    await user.type(screen.getByLabelText("Поиск"), "zzz");
    expect(screen.getByText("Ничего не найдено")).toBeInTheDocument();
    expect(screen.queryByText("Нет данных")).not.toBeInTheDocument();
  });

  it("shows empty state", async () => {
    server.use(http.get(URL, () => HttpResponse.json([])));
    renderWithProviders(<ElementsPage />);
    expect(await screen.findByText("Нет данных")).toBeInTheDocument();
  });

  it("shows error", async () => {
    server.use(http.get(URL, () => HttpResponse.json({ message: "boom" }, { status: 500 })));
    renderWithProviders(<ElementsPage />);
    expect(await screen.findByRole("alert")).toBeInTheDocument();
  });
});

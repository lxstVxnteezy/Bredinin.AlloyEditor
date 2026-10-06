import { describe, expect, it } from "vitest";
import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { http, HttpResponse } from "msw";
import { server } from "../../test/server";
import { renderWithProviders } from "../../test/renderWithProviders";
import { SystemsPage } from "./SystemsPage";

const systems = [{ id: "s1", baseElementId: "e1", name: "Стали", description: "Fe-based" }];
const elements = [
  { id: "e1", name: "Железо", symbol: "Fe", isBaseForAlloySystem: true, atomicNumber: 26,
    atomicWeight: 55, group: 8, period: 4, density: 7.8 },
];

function setup() {
  server.use(
    http.get("/api/gateway/alloy-system", () => HttpResponse.json(systems)),
    http.get("/api/gateway/dictionary/chemical-elements", () => HttpResponse.json(elements)),
  );
}

describe("SystemsPage", () => {
  it("shows systems with resolved base element and empty selection", async () => {
    setup();
    renderWithProviders(<SystemsPage />);
    expect(await screen.findByText("Стали")).toBeInTheDocument();
    expect(await screen.findByText("Fe (Железо)")).toBeInTheDocument();
    expect(screen.getByText("Выберите систему")).toBeInTheDocument();
  });

  it("loads grades on row click", async () => {
    setup();
    server.use(
      http.get("/api/gateway/alloy-grade/for-main-element/e1", () =>
        HttpResponse.json([
          { id: "g1", name: "Сталь 45", description: "конструкционная", chemicalCompositions: [{ chemicalElementId: "e1" }] },
        ]),
      ),
    );
    const user = userEvent.setup();
    renderWithProviders(<SystemsPage />);
    await user.click(await screen.findByText("Стали"));
    expect(await screen.findByText("Сталь 45")).toBeInTheDocument();
    expect(screen.getByText("конструкционная")).toBeInTheDocument();
  });

  it("shows error for grades query", async () => {
    setup();
    server.use(
      http.get("/api/gateway/alloy-grade/for-main-element/e1", () =>
        HttpResponse.json({ message: "x" }, { status: 500 }),
      ),
    );
    const user = userEvent.setup();
    renderWithProviders(<SystemsPage />);
    await user.click(await screen.findByText("Стали"));
    expect(await screen.findByRole("alert")).toBeInTheDocument();
  });
});

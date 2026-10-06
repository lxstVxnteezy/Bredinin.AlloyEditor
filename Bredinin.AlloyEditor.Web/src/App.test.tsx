import { beforeEach, describe, expect, it } from "vitest";
import { render, screen } from "@testing-library/react";
import App from "./App";

describe("App", () => {
  beforeEach(() => {
    localStorage.clear();
    window.history.pushState({}, "", "/grades");
  });

  it("sends an unauthenticated visitor to the login page", async () => {
    render(<App />);
    expect(await screen.findByRole("heading", { name: "Вход" })).toBeInTheDocument();
    expect(window.location.pathname).toBe("/login");
  });
});

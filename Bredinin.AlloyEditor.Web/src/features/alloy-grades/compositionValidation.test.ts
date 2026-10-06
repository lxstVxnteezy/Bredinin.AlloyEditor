import { describe, expect, it } from "vitest";
import { validateComposition } from "./compositionValidation";

const BASE = "al";

describe("validateComposition", () => {
  it("accepts an exact-100 total despite float error (0.2 + 83.9 + 15.9)", () => {
    const errors = validateComposition(
      [
        { chemicalElementId: BASE, exactValue: 83.9 },
        { chemicalElementId: "si", exactValue: 0.2 },
        { chemicalElementId: "fe", exactValue: 15.9 },
      ],
      BASE,
    );
    expect(errors).toEqual([]);
  });

  it("still rejects totals above 100", () => {
    const errors = validateComposition(
      [
        { chemicalElementId: BASE, exactValue: 99.9 },
        { chemicalElementId: "si", exactValue: 0.100001 },
      ],
      BASE,
    );
    expect(errors.some((e) => e.includes("100"))).toBe(true);
  });

  it("rejects empty list", () => {
    expect(validateComposition([], BASE).length).toBeGreaterThan(0);
  });

  it("rejects missing base element", () => {
    expect(validateComposition([{ chemicalElementId: "si", exactValue: 5 }], BASE).length).toBeGreaterThan(0);
  });

  it("rejects duplicate elements", () => {
    const errors = validateComposition(
      [
        { chemicalElementId: BASE, exactValue: 90 },
        { chemicalElementId: "si", exactValue: 2 },
        { chemicalElementId: "si", exactValue: 3 },
      ],
      BASE,
    );
    expect(errors.length).toBeGreaterThan(0);
  });

  it("rejects exact together with min or max", () => {
    expect(validateComposition([{ chemicalElementId: BASE, exactValue: 90, minValue: 1 }], BASE).length).toBeGreaterThan(0);
    expect(validateComposition([{ chemicalElementId: BASE, exactValue: 90, maxValue: 1 }], BASE).length).toBeGreaterThan(0);
  });

  it("rejects only min or only max", () => {
    expect(validateComposition([{ chemicalElementId: BASE, minValue: 1 }], BASE).length).toBeGreaterThan(0);
    expect(validateComposition([{ chemicalElementId: BASE, maxValue: 1 }], BASE).length).toBeGreaterThan(0);
  });

  it("rejects item without any value", () => {
    expect(validateComposition([{ chemicalElementId: BASE }], BASE).length).toBeGreaterThan(0);
  });

  it("rejects min greater than max", () => {
    expect(validateComposition([{ chemicalElementId: BASE, minValue: 5, maxValue: 1 }], BASE).length).toBeGreaterThan(0);
  });

  it("rejects sum of exact values and maxima above 100", () => {
    const errors = validateComposition(
      [
        { chemicalElementId: BASE, exactValue: 98 },
        { chemicalElementId: "si", minValue: 1, maxValue: 5 },
      ],
      BASE,
    );
    expect(errors.length).toBeGreaterThan(0);
  });

  it("rejects negative values", () => {
    expect(validateComposition([{ chemicalElementId: BASE, exactValue: -1 }], BASE).length).toBeGreaterThan(0);
  });

  it("accepts a correct composition", () => {
    expect(
      validateComposition(
        [
          { chemicalElementId: BASE, exactValue: 95 },
          { chemicalElementId: "si", minValue: 1, maxValue: 5 },
        ],
        BASE,
      ),
    ).toEqual([]);
  });
});

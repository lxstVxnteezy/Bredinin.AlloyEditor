import { describe, expect, it } from "vitest";
import { validateComposition } from "./compositionValidation";

const BASE = "al";

describe("validateComposition", () => {
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

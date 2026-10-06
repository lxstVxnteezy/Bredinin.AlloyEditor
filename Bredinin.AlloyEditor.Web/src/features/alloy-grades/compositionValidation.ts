import type { Composition } from "../../api";

const has = (v: number | null | undefined): v is number => v !== null && v !== undefined;

export function validateComposition(items: Composition[], baseElementId: string): string[] {
  const errors: string[] = [];
  if (items.length === 0) return ["Добавьте хотя бы один элемент в состав"];

  if (!items.some((i) => i.chemicalElementId === baseElementId)) {
    errors.push("В составе должен быть базовый элемент выбранной системы");
  }

  const ids = items.map((i) => i.chemicalElementId);
  if (new Set(ids).size !== ids.length) errors.push("Элементы в составе не должны повторяться");

  // Sum in integer micro-units: the backend uses decimal, doubles would reject e.g. 0.2 + 83.9 + 15.9.
  const UNIT = 1e6;
  let total = 0;
  items.forEach((i, idx) => {
    const n = idx + 1;
    const { exactValue, minValue, maxValue } = i;
    const values = [exactValue, minValue, maxValue];
    if (values.some((v) => has(v) && v < 0)) {
      errors.push(`Строка ${n}: значения не могут быть отрицательными`);
    }
    if (has(exactValue)) {
      if (has(minValue) || has(maxValue)) {
        errors.push(`Строка ${n}: укажите либо точное значение, либо диапазон`);
      } else {
        total += Math.round(exactValue * UNIT);
      }
    } else if (has(minValue) !== has(maxValue)) {
      errors.push(`Строка ${n}: для диапазона укажите и минимум, и максимум`);
    } else if (has(minValue) && has(maxValue)) {
      if (minValue > maxValue) errors.push(`Строка ${n}: минимум не может быть больше максимума`);
      total += Math.round(maxValue * UNIT);
    } else {
      errors.push(`Строка ${n}: укажите значение`);
    }
  });

  if (total > 100 * UNIT) errors.push("Сумма точных значений и максимумов не должна превышать 100%");
  return errors;
}

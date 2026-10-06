import { Button, IconButton, MenuItem, Stack, TextField, ToggleButton, ToggleButtonGroup } from "@mui/material";
import DeleteIcon from "@mui/icons-material/Delete";
import AddIcon from "@mui/icons-material/Add";
import type { ChemicalElement } from "../../api";

export interface CompositionRow {
  key: number;
  elementId: string;
  mode: "exact" | "range";
  exact: string;
  min: string;
  max: string;
}

let nextKey = 1;
export const newRow = (elementId = ""): CompositionRow => ({
  key: nextKey++,
  elementId,
  mode: "exact",
  exact: "",
  min: "",
  max: "",
});

interface Props {
  rows: CompositionRow[];
  elements: ChemicalElement[];
  disabled?: boolean;
  onChange(rows: CompositionRow[]): void;
}

export function CompositionRows({ rows, elements, disabled, onChange }: Props) {
  const update = (key: number, patch: Partial<CompositionRow>) =>
    onChange(rows.map((r) => (r.key === key ? { ...r, ...patch } : r)));

  const numberField = (r: CompositionRow, field: "exact" | "min" | "max", label: string, width: number) => (
    <TextField
      size="small"
      label={label}
      value={r[field]}
      disabled={disabled}
      onChange={(e) => update(r.key, { [field]: e.target.value })}
      slotProps={{ htmlInput: { inputMode: "decimal" } }}
      sx={{ width }}
    />
  );

  return (
    <Stack spacing={1.5}>
      {rows.map((r) => (
        <Stack key={r.key} direction="row" spacing={1} sx={{ alignItems: "center" }}>
          <TextField
            select
            size="small"
            label="Элемент"
            value={r.elementId}
            disabled={disabled}
            onChange={(e) => update(r.key, { elementId: e.target.value })}
            sx={{ minWidth: 160 }}
          >
            {elements.map((el) => (
              <MenuItem key={el.id} value={el.id}>
                {el.symbol} — {el.name}
              </MenuItem>
            ))}
          </TextField>
          <ToggleButtonGroup
            exclusive
            size="small"
            value={r.mode}
            disabled={disabled}
            onChange={(_, mode: CompositionRow["mode"] | null) => mode && update(r.key, { mode })}
          >
            <ToggleButton value="exact">Точное значение</ToggleButton>
            <ToggleButton value="range">Диапазон</ToggleButton>
          </ToggleButtonGroup>
          {r.mode === "exact" ? (
            numberField(r, "exact", "Значение", 110)
          ) : (
            <>
              {numberField(r, "min", "Мин", 90)}
              {numberField(r, "max", "Макс", 90)}
            </>
          )}
          <IconButton
            aria-label="Удалить элемент"
            size="small"
            disabled={disabled}
            onClick={() => onChange(rows.filter((x) => x.key !== r.key))}
          >
            <DeleteIcon fontSize="small" />
          </IconButton>
        </Stack>
      ))}
      <div>
        <Button size="small" startIcon={<AddIcon />} disabled={disabled} onClick={() => onChange([...rows, newRow()])}>
          Добавить элемент
        </Button>
      </div>
    </Stack>
  );
}

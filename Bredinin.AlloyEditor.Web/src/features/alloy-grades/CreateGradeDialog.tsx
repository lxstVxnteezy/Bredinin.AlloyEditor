import { useState } from "react";
import {
  Alert, Button, Dialog, DialogActions, DialogContent, DialogTitle, MenuItem, Stack, TextField, Typography,
} from "@mui/material";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createGrade, toApiError, type Composition } from "../../api";
import { useElements, useSystems } from "../../api/queries";
import { useNotify } from "../../components/Notifications";
import { CompositionRows, newRow, type CompositionRow } from "./CompositionRows";
import { validateComposition } from "./compositionValidation";

interface Props {
  open: boolean;
  onClose(): void;
}

// Empty text -> undefined; invalid text -> NaN. Accepts a decimal comma.
const parse = (s: string): number | undefined => {
  const t = s.trim().replace(",", ".");
  if (t === "") return undefined;
  return /^-?(\d+\.?\d*|\.\d+)$/.test(t) ? Number(t) : NaN;
};

function buildComposition(rows: CompositionRow[]): { items: Composition[]; errors: string[] } {
  const items: Composition[] = [];
  const errors: string[] = [];
  rows.forEach((r, idx) => {
    const n = idx + 1;
    if (!r.elementId) errors.push(`Строка ${n}: выберите элемент`);
    const nums = (r.mode === "exact" ? [r.exact] : [r.min, r.max]).map(parse);
    if (nums.some((v) => v !== undefined && Number.isNaN(v))) {
      errors.push(`Строка ${n}: введите корректное число`);
      return;
    }
    items.push(
      r.mode === "exact"
        ? { chemicalElementId: r.elementId, exactValue: nums[0] }
        : { chemicalElementId: r.elementId, minValue: nums[0], maxValue: nums[1] },
    );
  });
  return { items, errors };
}

export function CreateGradeDialog({ open, onClose }: Props) {
  const systems = useSystems();
  const elements = useElements();
  const { notify } = useNotify();
  const queryClient = useQueryClient();

  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [systemId, setSystemId] = useState("");
  const [rows, setRows] = useState<CompositionRow[]>([]);
  const [errors, setErrors] = useState<string[]>([]);

  const create = useMutation({
    mutationFn: createGrade,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["grades"] });
      notify("Марка создана");
      onClose();
    },
  });

  const selectSystem = (id: string) => {
    setSystemId(id);
    const base = systems.data?.find((s) => s.id === id)?.baseElementId;
    if (base && rows.every((r) => r.elementId === "")) setRows([newRow(base)]);
  };

  const submit = () => {
    if (create.isPending) return;
    const base = systems.data?.find((s) => s.id === systemId)?.baseElementId ?? "";
    const problems: string[] = [];
    if (name.trim() === "") problems.push("Укажите название марки");
    if (!systemId) problems.push("Выберите систему сплава");
    const { items, errors: rowErrors } = buildComposition(rows);
    problems.push(...rowErrors);
    if (rowErrors.length === 0) problems.push(...validateComposition(items, base));
    setErrors(problems);
    if (problems.length > 0) return;
    create.mutate({
      name: name.trim(),
      description: description.trim() || undefined,
      alloySystemId: systemId,
      chemicalCompositions: items,
      heatTreatments: [],
    });
  };

  const close = () => {
    if (!create.isPending) onClose();
  };

  return (
    <Dialog open={open} onClose={close} fullWidth maxWidth="md">
      <DialogTitle>Создание марки</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ pt: 1 }}>
          <TextField
            label="Название"
            required
            size="small"
            value={name}
            disabled={create.isPending}
            onChange={(e) => setName(e.target.value)}
          />
          <TextField
            label="Описание"
            size="small"
            multiline
            value={description}
            disabled={create.isPending}
            onChange={(e) => setDescription(e.target.value)}
          />
          <TextField
            select
            required
            size="small"
            label="Система сплава"
            value={systemId}
            disabled={create.isPending}
            onChange={(e) => selectSystem(e.target.value)}
          >
            {(systems.data ?? []).map((s) => (
              <MenuItem key={s.id} value={s.id}>
                {s.name}
              </MenuItem>
            ))}
          </TextField>
          <Typography variant="subtitle2">Химический состав, %</Typography>
          <CompositionRows
            rows={rows}
            elements={elements.data ?? []}
            disabled={create.isPending}
            onChange={setRows}
          />
          {errors.length > 0 && (
            <Alert severity="error">
              <Stack component="ul" sx={{ m: 0, pl: 2 }}>
                {errors.map((e) => (
                  <li key={e}>{e}</li>
                ))}
              </Stack>
            </Alert>
          )}
          {create.error && (
            <Alert severity="error">{toApiError(create.error).message || "Не удалось создать марку"}</Alert>
          )}
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={close} disabled={create.isPending}>
          Отмена
        </Button>
        <Button variant="contained" onClick={submit} disabled={create.isPending}>
          Создать
        </Button>
      </DialogActions>
    </Dialog>
  );
}

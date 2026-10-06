import {
  Box, Button, Dialog, DialogActions, DialogContent, DialogTitle, Table, TableBody, TableCell,
  TableHead, TableRow, Typography,
} from "@mui/material";
import type { AlloyGrade, ChemicalElement } from "../../api";

const dash = (v: number | null | undefined) => (v === null || v === undefined ? "—" : String(v));

const range = (min?: number | null, max?: number | null, exact?: number | null) => [
  dash(min),
  dash(max),
  dash(exact),
];

interface Props {
  grade: AlloyGrade | null;
  systemName?: string;
  elements: ChemicalElement[] | undefined;
  onClose(): void;
}

export function GradeDetails({ grade, systemName, elements, onClose }: Props) {
  const symbol = (id: string) => elements?.find((e) => e.id === id)?.symbol ?? id;
  return (
    <Dialog open={grade !== null} onClose={onClose} fullWidth maxWidth="md">
      {grade && (
        <>
          <DialogTitle>{grade.name}</DialogTitle>
          <DialogContent>
            <Typography color="text.secondary" gutterBottom>
              Система сплава: {systemName ?? grade.alloySystemId}
            </Typography>
            {grade.description ? <Typography gutterBottom>{grade.description}</Typography> : null}
            <Box sx={{ mt: 2 }}>
              <Typography variant="h6" component="h2" gutterBottom>
                Состав
              </Typography>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>Элемент</TableCell>
                    <TableCell>Мин</TableCell>
                    <TableCell>Макс</TableCell>
                    <TableCell>Точное</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {grade.chemicalCompositions.map((c, i) => (
                    <TableRow key={c.id ?? `${c.chemicalElementId}-${i}`}>
                      <TableCell>{symbol(c.chemicalElementId)}</TableCell>
                      {range(c.minValue, c.maxValue, c.exactValue).map((v, j) => (
                        <TableCell key={j}>{v}</TableCell>
                      ))}
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </Box>
            {grade.heatTreatments.length > 0 && (
              <Box sx={{ mt: 3 }}>
                <Typography variant="h6" component="h2" gutterBottom>
                  Термообработка
                </Typography>
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell>Вид</TableCell>
                      <TableCell>Температура (мин / макс / точная)</TableCell>
                      <TableCell>Выдержка (мин / макс / точная)</TableCell>
                      <TableCell>Охлаждающая среда</TableCell>
                      <TableCell>По умолчанию</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {grade.heatTreatments.map((h) => (
                      <TableRow key={h.id}>
                        <TableCell>{h.heatTreatmentTypeName}</TableCell>
                        <TableCell>
                          {range(h.temperatureMin, h.temperatureMax, h.temperatureExact).join(" / ")}
                        </TableCell>
                        <TableCell>
                          {range(h.holdingTimeMin, h.holdingTimeMax, h.holdingTimeExact).join(" / ")}
                        </TableCell>
                        <TableCell>{h.coolingMedium || "—"}</TableCell>
                        <TableCell>{h.isDefault ? "Да" : "Нет"}</TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </Box>
            )}
            {grade.mechanicalProperties.length > 0 && (
              <Box sx={{ mt: 3 }}>
                <Typography variant="h6" component="h2" gutterBottom>
                  Механические свойства
                </Typography>
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell>Свойство</TableCell>
                      <TableCell>Ед.</TableCell>
                      <TableCell>Мин</TableCell>
                      <TableCell>Макс</TableCell>
                      <TableCell>Точное</TableCell>
                      <TableCell>Условие</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {grade.mechanicalProperties.map((m) => (
                      <TableRow key={m.id}>
                        <TableCell>{m.propertyTypeName}</TableCell>
                        <TableCell>{m.propertyTypeUnit}</TableCell>
                        {range(m.valueMin, m.valueMax, m.valueExact).map((v, j) => (
                          <TableCell key={j}>{v}</TableCell>
                        ))}
                        <TableCell>{m.condition || "—"}</TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </Box>
            )}
          </DialogContent>
          <DialogActions>
            <Button onClick={onClose}>Закрыть</Button>
          </DialogActions>
        </>
      )}
    </Dialog>
  );
}

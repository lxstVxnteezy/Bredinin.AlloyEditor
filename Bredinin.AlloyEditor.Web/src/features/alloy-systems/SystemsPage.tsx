import { useState } from "react";
import {
  Box, CircularProgress, List, ListItem, ListItemText, Paper, Table, TableBody, TableCell,
  TableContainer, TableHead, TableRow, Typography,
} from "@mui/material";
import { useElements, useGradesForMainElement, useSystems } from "../../api/queries";
import { ErrorAlert } from "../../components/ErrorAlert";

function Grades({ baseElementId }: { baseElementId: string }) {
  const { data, isLoading, error } = useGradesForMainElement(baseElementId);
  if (isLoading) return <CircularProgress aria-label="Загрузка марок" />;
  if (error) return <ErrorAlert error={error} />;
  if (!data || data.length === 0) return <Typography color="text.secondary">Нет данных</Typography>;
  return (
    <List dense>
      {data.map((g) => (
        <ListItem key={g.id} divider>
          <ListItemText
            primary={g.name}
            secondary={
              <>
                {g.description ? <span>{g.description}</span> : null}
                <br />
                <span>Элементов в составе: {g.chemicalCompositions.length}</span>
              </>
            }
          />
        </ListItem>
      ))}
    </List>
  );
}

export function SystemsPage() {
  const systems = useSystems();
  const elements = useElements();
  const [selected, setSelected] = useState<string | null>(null);
  const selectedSystem = systems.data?.find((s) => s.id === selected);

  const baseLabel = (id: string) => {
    const el = elements.data?.find((e) => e.id === id);
    return el ? `${el.symbol} (${el.name})` : id;
  };

  return (
    <Box>
      <Typography variant="h5" component="h1" gutterBottom>
        Системы сплавов
      </Typography>
      {systems.isLoading ? (
        <CircularProgress aria-label="Загрузка" />
      ) : systems.error ? (
        <ErrorAlert error={systems.error} />
      ) : !systems.data || systems.data.length === 0 ? (
        <Typography color="text.secondary">Нет данных</Typography>
      ) : (
        <TableContainer component={Paper}>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Название</TableCell>
                <TableCell>Базовый элемент</TableCell>
                <TableCell>Описание</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {systems.data.map((s) => (
                <TableRow
                  key={s.id}
                  hover
                  selected={s.id === selected}
                  onClick={() => setSelected(s.id)}
                  sx={{ cursor: "pointer" }}
                >
                  <TableCell>{s.name}</TableCell>
                  <TableCell>{baseLabel(s.baseElementId)}</TableCell>
                  <TableCell>{s.description}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      )}
      <Box sx={{ mt: 3 }}>
        {selectedSystem ? (
          <>
            <Typography variant="h6" gutterBottom>
              Марки: {selectedSystem.name}
            </Typography>
            <Grades baseElementId={selectedSystem.baseElementId} />
          </>
        ) : (
          <Typography color="text.secondary">Выберите систему</Typography>
        )}
      </Box>
    </Box>
  );
}

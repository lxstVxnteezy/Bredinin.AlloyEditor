import { useMemo, useState } from "react";
import {
  Box, Chip, CircularProgress, Paper, Table, TableBody, TableCell, TableContainer,
  TableHead, TableRow, TextField, Typography,
} from "@mui/material";
import { useElements } from "../../api/queries";
import { ErrorAlert } from "../../components/ErrorAlert";

export function ElementsPage() {
  const { data, isLoading, error } = useElements();
  const [search, setSearch] = useState("");

  const rows = useMemo(() => {
    const q = search.trim().toLowerCase();
    return (data ?? []).filter(
      (e) => !q || e.symbol.toLowerCase().includes(q) || e.name.toLowerCase().includes(q),
    );
  }, [data, search]);

  return (
    <Box>
      <Typography variant="h5" component="h1" gutterBottom>
        Элементы
      </Typography>
      <TextField
        label="Поиск"
        size="small"
        value={search}
        onChange={(e) => setSearch(e.target.value)}
        sx={{ mb: 2 }}
      />
      {isLoading ? (
        <CircularProgress aria-label="Загрузка" />
      ) : error ? (
        <ErrorAlert error={error} />
      ) : !data || data.length === 0 ? (
        <Typography color="text.secondary">Нет данных</Typography>
      ) : rows.length === 0 ? (
        <Typography color="text.secondary">Ничего не найдено</Typography>
      ) : (
        <TableContainer component={Paper}>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Символ</TableCell>
                <TableCell>Название</TableCell>
                <TableCell>Атомный номер</TableCell>
                <TableCell>Атомная масса</TableCell>
                <TableCell>Группа</TableCell>
                <TableCell>Период</TableCell>
                <TableCell>Плотность</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {rows.map((e) => (
                <TableRow key={e.id}>
                  <TableCell>{e.symbol}</TableCell>
                  <TableCell>
                    {e.name}{" "}
                    {e.isBaseForAlloySystem && <Chip size="small" color="primary" label="Базовый" />}
                  </TableCell>
                  <TableCell>{e.atomicNumber}</TableCell>
                  <TableCell>{e.atomicWeight}</TableCell>
                  <TableCell>{e.group}</TableCell>
                  <TableCell>{e.period}</TableCell>
                  <TableCell>{e.density}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      )}
    </Box>
  );
}

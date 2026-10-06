import { useState } from "react";
import {
  Box, Button, CircularProgress, IconButton, MenuItem, Paper, Stack, Table, TableBody, TableCell,
  TableContainer, TableHead, TableRow, TextField, Tooltip, Typography,
} from "@mui/material";
import DeleteIcon from "@mui/icons-material/Delete";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { deleteGrade, toApiError, type AlloyGrade } from "../../api";
import { useElements, useGrades, useSystems } from "../../api/queries";
import { ConfirmDialog } from "../../components/ConfirmDialog";
import { ErrorAlert } from "../../components/ErrorAlert";
import { useNotify } from "../../components/Notifications";
import { CreateGradeDialog } from "./CreateGradeDialog";
import { GradeDetails } from "./GradeDetails";

const ALL = "";

export function GradesPage() {
  const grades = useGrades();
  const systems = useSystems();
  const elements = useElements();
  const { notify } = useNotify();
  const queryClient = useQueryClient();

  const [search, setSearch] = useState("");
  const [systemId, setSystemId] = useState(ALL);
  const [openId, setOpenId] = useState<string | null>(null);
  const [toDelete, setToDelete] = useState<AlloyGrade | null>(null);
  const [creating, setCreating] = useState(false);

  const remove = useMutation({
    mutationFn: deleteGrade,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["grades"] });
      notify("Марка удалена");
    },
    onError: (e) => notify(toApiError(e).message || "Не удалось удалить марку", "error"),
    onSettled: () => setToDelete(null),
  });

  const systemName = (id: string) => systems.data?.find((s) => s.id === id)?.name ?? id;

  const query = search.trim().toLowerCase();
  const filtered = (grades.data ?? []).filter(
    (g) =>
      (systemId === ALL || g.alloySystemId === systemId) &&
      (query === "" || g.name.toLowerCase().includes(query)),
  );
  const opened = grades.data?.find((g) => g.id === openId) ?? null;

  return (
    <Box>
      <Typography variant="h5" component="h1" gutterBottom>
        Марки
      </Typography>
      <Stack direction="row" spacing={2} sx={{ mb: 2, alignItems: "center" }}>
        <TextField
          size="small"
          label="Поиск по названию"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
        <TextField
          select
          size="small"
          label="Система сплава"
          value={systemId}
          onChange={(e) => setSystemId(e.target.value)}
          sx={{ minWidth: 220 }}
        >
          <MenuItem value={ALL}>Все системы</MenuItem>
          {(systems.data ?? []).map((s) => (
            <MenuItem key={s.id} value={s.id}>
              {s.name}
            </MenuItem>
          ))}
        </TextField>
        <Button variant="contained" onClick={() => setCreating(true)}>
          Создать
        </Button>
        <Tooltip title="Недоступно: нет эндпоинта в Gateway">
          <span>
            <Button disabled>Редактировать</Button>
          </span>
        </Tooltip>
      </Stack>
      {grades.isLoading ? (
        <CircularProgress aria-label="Загрузка" />
      ) : grades.error ? (
        <ErrorAlert error={grades.error} />
      ) : !grades.data || grades.data.length === 0 ? (
        <Typography color="text.secondary">Марок пока нет</Typography>
      ) : filtered.length === 0 ? (
        <Typography color="text.secondary">Ничего не найдено</Typography>
      ) : (
        <TableContainer component={Paper}>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Название</TableCell>
                <TableCell>Система сплава</TableCell>
                <TableCell>Описание</TableCell>
                <TableCell>Элементов в составе</TableCell>
                <TableCell />
              </TableRow>
            </TableHead>
            <TableBody>
              {filtered.map((g) => (
                <TableRow
                  key={g.id}
                  hover
                  selected={g.id === openId}
                  tabIndex={0}
                  aria-selected={g.id === openId}
                  onClick={() => setOpenId(g.id)}
                  onKeyDown={(e) => {
                    if (e.target !== e.currentTarget) return;
                    if (e.key === "Enter" || e.key === " ") {
                      e.preventDefault();
                      setOpenId(g.id);
                    }
                  }}
                  sx={{ cursor: "pointer" }}
                >
                  <TableCell>{g.name}</TableCell>
                  <TableCell>{systemName(g.alloySystemId)}</TableCell>
                  <TableCell>{g.description}</TableCell>
                  <TableCell>{g.chemicalCompositions.length}</TableCell>
                  <TableCell align="right">
                    <IconButton
                      aria-label="Удалить"
                      size="small"
                      onClick={(e) => {
                        e.stopPropagation();
                        setToDelete(g);
                      }}
                    >
                      <DeleteIcon fontSize="small" />
                    </IconButton>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      )}
      {creating && <CreateGradeDialog open onClose={() => setCreating(false)} />}
      <GradeDetails
        grade={opened}
        systemName={opened ? systemName(opened.alloySystemId) : undefined}
        elements={elements.data}
        onClose={() => setOpenId(null)}
      />
      <ConfirmDialog
        open={toDelete !== null}
        title="Удалить марку"
        text={`Удалить марку «${toDelete?.name ?? ""}»? Действие необратимо.`}
        confirmDisabled={remove.isPending}
        onConfirm={() => {
          if (toDelete && !remove.isPending) remove.mutate(toDelete.id);
        }}
        onCancel={() => {
          if (!remove.isPending) setToDelete(null);
        }}
      />
    </Box>
  );
}

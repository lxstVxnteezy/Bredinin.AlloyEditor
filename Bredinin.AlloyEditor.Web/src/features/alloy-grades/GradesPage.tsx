import { useEffect, useState } from "react";
import {
  Box, Button, Chip, CircularProgress, IconButton, InputAdornment, MenuItem, Stack, Table, TableBody,
  TableCell, TableContainer, TableHead, TablePagination, TableRow, TextField, Tooltip, Typography,
} from "@mui/material";
import AddIcon from "@mui/icons-material/AddRounded";
import DeleteIcon from "@mui/icons-material/DeleteOutlineRounded";
import SearchIcon from "@mui/icons-material/SearchRounded";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { deleteGrade, toApiError, type AlloyGrade } from "../../api";
import { useElements, useGrades, useSystems } from "../../api/queries";
import { ConfirmDialog } from "../../components/ConfirmDialog";
import { ErrorAlert } from "../../components/ErrorAlert";
import { useNotify } from "../../components/Notifications";
import { CreateGradeDialog } from "./CreateGradeDialog";
import { GradeDetails } from "./GradeDetails";

const ALL = "";
const SEARCH_DELAY_MS = 300;
const PAGE_SIZES = [10, 20, 50];

const gradesCount = (n: number) => {
  const tail = n % 100;
  const last = n % 10;
  if (tail >= 11 && tail <= 14) return `${n} марок`;
  if (last === 1) return `${n} марка`;
  if (last >= 2 && last <= 4) return `${n} марки`;
  return `${n} марок`;
};

function EmptyState({ children }: { children: string }) {
  return (
    <Box
      sx={{
        py: 8,
        textAlign: "center",
        color: "text.secondary",
        border: "1px dashed",
        borderColor: "divider",
        borderRadius: "14px",
        bgcolor: "rgba(255,255,255,0.6)",
      }}
    >
      <Typography color="inherit">{children}</Typography>
    </Box>
  );
}

export function GradesPage() {
  const [searchInput, setSearchInput] = useState("");
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(20);
  const grades = useGrades({ search, page: page + 1, pageSize });
  const systems = useSystems();
  const elements = useElements();
  const { notify } = useNotify();
  const queryClient = useQueryClient();

  const [systemId, setSystemId] = useState(ALL);
  const [openId, setOpenId] = useState<string | null>(null);
  const [toDelete, setToDelete] = useState<AlloyGrade | null>(null);
  const [creating, setCreating] = useState(false);

  // The search runs on the server: send the trimmed text after a short pause and go back to the first page.
  useEffect(() => {
    const timer = setTimeout(() => {
      const next = searchInput.trim();
      if (next === search) return;
      setSearch(next);
      setPage(0);
    }, SEARCH_DELAY_MS);
    return () => clearTimeout(timer);
  }, [searchInput, search]);

  const remove = useMutation({
    mutationFn: deleteGrade,
    onSuccess: async () => {
      // Deleting the only item of a later page would leave an empty page: go back one page.
      if (items.length === 1 && page > 0) setPage(page - 1);
      await queryClient.invalidateQueries({ queryKey: ["grades"] });
      notify("Марка удалена");
    },
    onError: (e) => notify(toApiError(e).message || "Не удалось удалить марку", "error"),
    onSettled: () => setToDelete(null),
  });

  const systemName = (id: string) => systems.data?.find((s) => s.id === id)?.name ?? id;

  // Gateway filters by name only, so the system filter applies to the loaded page.
  const items = grades.data?.items ?? [];
  const filtered = items.filter((g) => systemId === ALL || g.alloySystemId === systemId);
  const opened = items.find((g) => g.id === openId) ?? null;
  const isEmptyCatalog = grades.data?.totalCount === 0 && search === "";

  return (
    <Box>
      <Stack
        direction="row"
        sx={{ mb: 3, gap: 2, alignItems: "flex-end", justifyContent: "space-between", flexWrap: "wrap" }}
      >
        <Box>
          <Typography variant="h5" component="h1">
            Марки
          </Typography>
          <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5, minHeight: 20 }}>
            {grades.data
              ? search
                ? `Найдено: ${gradesCount(grades.data.totalCount)}`
                : `В каталоге ${gradesCount(grades.data.totalCount)}`
              : ""}
          </Typography>
        </Box>
        <Stack direction="row" spacing={1.5}>
          <Tooltip title="Недоступно: нет эндпоинта в Gateway">
            <span>
              <Button variant="outlined" disabled>
                Редактировать
              </Button>
            </span>
          </Tooltip>
          <Button variant="contained" startIcon={<AddIcon />} onClick={() => setCreating(true)}>
            Создать
          </Button>
        </Stack>
      </Stack>

      <Stack direction={{ xs: "column", sm: "row" }} spacing={1.5} sx={{ mb: 2.5 }}>
        <TextField
          fullWidth
          placeholder="Поиск по названию"
          value={searchInput}
          onChange={(e) => setSearchInput(e.target.value)}
          slotProps={{
            htmlInput: { "aria-label": "Поиск по названию" },
            input: {
              startAdornment: (
                <InputAdornment position="start">
                  <SearchIcon fontSize="small" sx={{ color: "text.secondary" }} />
                </InputAdornment>
              ),
            },
          }}
        />
        <Tooltip title="Фильтр применяется к текущей странице" placement="top">
          <TextField
            select
            label="Система сплава"
            value={systemId}
            onChange={(e) => setSystemId(e.target.value)}
            slotProps={{ select: { displayEmpty: true }, inputLabel: { shrink: true } }}
            sx={{ minWidth: { sm: 260 }, flexShrink: 0 }}
          >
            <MenuItem value={ALL}>Все системы</MenuItem>
            {(systems.data ?? []).map((s) => (
              <MenuItem key={s.id} value={s.id}>
                {s.name}
              </MenuItem>
            ))}
          </TextField>
        </Tooltip>
      </Stack>

      {grades.isLoading ? (
        <Box sx={{ py: 10, display: "flex", justifyContent: "center" }}>
          <CircularProgress aria-label="Загрузка" />
        </Box>
      ) : grades.error ? (
        <ErrorAlert error={grades.error} />
      ) : isEmptyCatalog ? (
        <EmptyState>Марок пока нет</EmptyState>
      ) : filtered.length === 0 ? (
        <EmptyState>Ничего не найдено</EmptyState>
      ) : (
        <TableContainer>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>Название</TableCell>
                <TableCell>Система сплава</TableCell>
                <TableCell>Описание</TableCell>
                <TableCell align="right">Элементов в составе</TableCell>
                <TableCell padding="checkbox" />
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
                  <TableCell sx={{ fontWeight: 600, whiteSpace: "nowrap" }}>{g.name}</TableCell>
                  <TableCell>
                    <Chip size="small" variant="outlined" label={systemName(g.alloySystemId)} />
                  </TableCell>
                  <TableCell sx={{ color: "text.secondary", maxWidth: 480 }}>
                    <Box
                      sx={{
                        display: "-webkit-box",
                        WebkitLineClamp: 2,
                        WebkitBoxOrient: "vertical",
                        overflow: "hidden",
                      }}
                    >
                      {g.description}
                    </Box>
                  </TableCell>
                  <TableCell align="right" sx={{ fontVariantNumeric: "tabular-nums" }}>
                    {g.chemicalCompositions.length}
                  </TableCell>
                  <TableCell padding="checkbox" align="right" sx={{ pr: 1.5 }}>
                    <IconButton
                      aria-label="Удалить"
                      size="small"
                      onClick={(e) => {
                        e.stopPropagation();
                        setToDelete(g);
                      }}
                      sx={{ color: "text.secondary", "&:hover": { color: "error.main", bgcolor: "rgba(184,58,58,0.08)" } }}
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
      {grades.data && !isEmptyCatalog && (
        <TablePagination
          component="div"
          count={grades.data.totalCount}
          page={page}
          rowsPerPage={pageSize}
          rowsPerPageOptions={PAGE_SIZES}
          onPageChange={(_, next) => setPage(next)}
          onRowsPerPageChange={(e) => {
            setPageSize(Number(e.target.value));
            setPage(0);
          }}
          labelRowsPerPage="Строк на странице"
          labelDisplayedRows={({ from, to, count }) => `${from}–${to} из ${count}`}
          getItemAriaLabel={(type) => (type === "next" ? "Следующая страница" : "Предыдущая страница")}
        />
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

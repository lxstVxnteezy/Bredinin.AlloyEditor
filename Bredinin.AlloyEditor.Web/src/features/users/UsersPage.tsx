import { useState } from "react";
import {
  Box, Button, CircularProgress, IconButton, Paper, Stack, Table, TableBody, TableCell,
  TableContainer, TableHead, TableRow, TextField, Tooltip, Typography,
} from "@mui/material";
import DeleteIcon from "@mui/icons-material/Delete";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { deleteUser, getUsers, revokeAccess, toApiError, type User } from "../../api";
import { useAuth } from "../../auth/useAuth";
import { ConfirmDialog } from "../../components/ConfirmDialog";
import { ErrorAlert } from "../../components/ErrorAlert";
import { useNotify } from "../../components/Notifications";
import { roleName, SELF_HINT } from "./roles";
import { UserDialog } from "./UserDialog";

const fullName = (u: User) => [u.lastName, u.firstName, u.secondName].filter(Boolean).join(" ");

export function UsersPage() {
  const users = useQuery({ queryKey: ["users"], queryFn: getUsers });
  const { notify } = useNotify();
  const { userId } = useAuth();
  const queryClient = useQueryClient();

  const [search, setSearch] = useState("");
  const [creating, setCreating] = useState(false);
  const [editing, setEditing] = useState<User | null>(null);
  const [toDelete, setToDelete] = useState<User | null>(null);
  const [toRevoke, setToRevoke] = useState<User | null>(null);

  const remove = useMutation({
    mutationFn: deleteUser,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["users"] });
      notify("Пользователь удалён");
    },
    onError: (e) => notify(toApiError(e).message || "Не удалось удалить пользователя", "error"),
    onSettled: () => setToDelete(null),
  });

  const revoke = useMutation({
    mutationFn: revokeAccess,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["users"] });
      notify("Доступ отозван");
    },
    onError: (e) => notify(toApiError(e).message || "Не удалось отозвать доступ", "error"),
    onSettled: () => setToRevoke(null),
  });

  const isSelf = (u: User) => userId !== null && u.id.toLowerCase() === userId.toLowerCase();

  const query = search.trim().toLowerCase();
  const filtered = (users.data ?? []).filter(
    (u) => query === "" || `${u.login} ${fullName(u)}`.toLowerCase().includes(query),
  );

  return (
    <Box>
      <Typography variant="h5" component="h1" gutterBottom>
        Пользователи
      </Typography>
      <Stack direction="row" spacing={2} sx={{ mb: 2, alignItems: "center" }}>
        <TextField
          size="small"
          label="Поиск"
          placeholder="Логин или ФИО"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
        <Button variant="contained" onClick={() => setCreating(true)}>
          Создать
        </Button>
      </Stack>
      {users.isLoading ? (
        <CircularProgress aria-label="Загрузка" />
      ) : users.error ? (
        <ErrorAlert error={users.error} />
      ) : !users.data || users.data.length === 0 ? (
        <Typography color="text.secondary">Пользователей нет</Typography>
      ) : filtered.length === 0 ? (
        <Typography color="text.secondary">Ничего не найдено</Typography>
      ) : (
        <TableContainer component={Paper}>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Логин</TableCell>
                <TableCell>ФИО</TableCell>
                <TableCell>Возраст</TableCell>
                <TableCell>Роли</TableCell>
                <TableCell />
              </TableRow>
            </TableHead>
            <TableBody>
              {filtered.map((u) => (
                <TableRow key={u.id} hover>
                  <TableCell>{u.login}</TableCell>
                  <TableCell>{fullName(u)}</TableCell>
                  <TableCell>{u.age}</TableCell>
                  <TableCell>{u.roleIds.map(roleName).join(", ")}</TableCell>
                  <TableCell align="right" sx={{ whiteSpace: "nowrap" }}>
                    <Button size="small" onClick={() => setEditing(u)}>
                      Изменить
                    </Button>
                    <Tooltip title={isSelf(u) ? SELF_HINT : ""}>
                      <span>
                        <Button size="small" disabled={isSelf(u)} onClick={() => setToRevoke(u)}>
                          Отозвать доступ
                        </Button>
                      </span>
                    </Tooltip>
                    <Tooltip title="Недоступно: на сервере нет обработчика">
                      <span>
                        <Button size="small" disabled>
                          Сбросить пароль
                        </Button>
                      </span>
                    </Tooltip>
                    <Tooltip title={isSelf(u) ? SELF_HINT : ""}>
                      <span>
                        <IconButton aria-label="Удалить" size="small" disabled={isSelf(u)} onClick={() => setToDelete(u)}>
                          <DeleteIcon fontSize="small" />
                        </IconButton>
                      </span>
                    </Tooltip>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </TableContainer>
      )}
      {creating && <UserDialog onClose={() => setCreating(false)} />}
      {editing && <UserDialog user={editing} onClose={() => setEditing(null)} />}
      <ConfirmDialog
        open={toDelete !== null}
        title="Удалить пользователя"
        text={`Удалить пользователя «${toDelete?.login ?? ""}»? Действие необратимо.`}
        confirmDisabled={remove.isPending}
        onConfirm={() => {
          if (toDelete && !remove.isPending) remove.mutate(toDelete.id);
        }}
        onCancel={() => {
          if (!remove.isPending) setToDelete(null);
        }}
      />
      <ConfirmDialog
        open={toRevoke !== null}
        title="Отозвать доступ"
        text={`Отозвать доступ у пользователя «${toRevoke?.login ?? ""}»? Его токены станут недействительными.`}
        confirmDisabled={revoke.isPending}
        onConfirm={() => {
          if (toRevoke && !revoke.isPending) revoke.mutate(toRevoke.id);
        }}
        onCancel={() => {
          if (!revoke.isPending) setToRevoke(null);
        }}
      />
    </Box>
  );
}

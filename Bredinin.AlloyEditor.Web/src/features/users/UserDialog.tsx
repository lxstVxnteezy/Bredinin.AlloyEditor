import { useState } from "react";
import {
  Alert, Button, Checkbox, Dialog, DialogActions, DialogContent, DialogTitle, FormControlLabel,
  FormGroup, FormLabel, Stack, TextField, Tooltip,
} from "@mui/material";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createUser, toApiError, updateUser, type User } from "../../api";
import { useAuth } from "../../auth/useAuth";
import { useNotify } from "../../components/Notifications";
import { ADMIN_ROLE_ID, ROLES, SELF_HINT } from "./roles";

interface Props {
  /** Existing user to edit; omit to create a new one. */
  user?: User;
  onClose(): void;
}

export function UserDialog({ user, onClose }: Props) {
  const { notify } = useNotify();
  const queryClient = useQueryClient();
  const { userId } = useAuth();
  const isSelf = !!user && !!userId && user.id.toLowerCase() === userId.toLowerCase();
  const editing = user !== undefined;

  const [login, setLogin] = useState(user?.login ?? "");
  const [lastName, setLastName] = useState(user?.lastName ?? "");
  const [firstName, setFirstName] = useState(user?.firstName ?? "");
  const [secondName, setSecondName] = useState(user?.secondName ?? "");
  const [age, setAge] = useState(user ? String(user.age) : "");
  const [password, setPassword] = useState("");
  const [roleIds, setRoleIds] = useState<string[]>(user?.roleIds ?? []);
  const [errors, setErrors] = useState<string[]>([]);

  const save = useMutation({
    mutationFn: async () => {
      const common = {
        firstName: firstName.trim(),
        lastName: lastName.trim(),
        secondName: secondName.trim(),
        age: Number(age),
        // Keep ROLES order so the request body is deterministic; keep unknown ids untouched.
        roleIds: [
          ...ROLES.map((r) => r.id).filter((id) => roleIds.includes(id)),
          ...roleIds.filter((id) => !ROLES.some((r) => r.id === id)),
        ],
      };
      if (user) await updateUser({ id: user.id, ...common });
      else await createUser({ login: login.trim(), password, ...common });
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["users"] });
      notify(editing ? "Изменения сохранены" : "Пользователь создан");
      onClose();
    },
  });

  const submit = () => {
    if (save.isPending) return;
    const problems: string[] = [];
    if (!editing && login.trim() === "") problems.push("Укажите логин");
    if (lastName.trim() === "") problems.push("Укажите фамилию");
    if (firstName.trim() === "") problems.push("Укажите имя");
    if (secondName.trim() === "") problems.push("Укажите отчество");
    if (!/^\d+$/.test(age.trim()) || Number(age) > 150) problems.push("Укажите корректный возраст");
    if (!editing && password === "") problems.push("Укажите пароль");
    setErrors(problems);
    if (problems.length > 0) return;
    save.mutate();
  };

  const toggleRole = (id: string) =>
    setRoleIds((cur) => (cur.includes(id) ? cur.filter((x) => x !== id) : [...cur, id]));

  const close = () => {
    if (!save.isPending) onClose();
  };
  const busy = save.isPending;

  return (
    <Dialog
      open
      onClose={close}
      fullWidth
      maxWidth="sm"
    >
      <DialogTitle>{editing ? "Изменение пользователя" : "Создание пользователя"}</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ pt: 1 }}>
          <TextField
            label="Логин"
            required
            size="small"
            value={login}
            disabled={busy || editing}
            onChange={(e) => setLogin(e.target.value)}
            autoComplete="off"
          />
          <TextField label="Фамилия" required size="small" value={lastName} disabled={busy} onChange={(e) => setLastName(e.target.value)} />
          <TextField label="Имя" required size="small" value={firstName} disabled={busy} onChange={(e) => setFirstName(e.target.value)} />
          <TextField label="Отчество" required size="small" value={secondName} disabled={busy} onChange={(e) => setSecondName(e.target.value)} />
          <TextField
            label="Возраст"
            required
            size="small"
            value={age}
            disabled={busy}
            onChange={(e) => setAge(e.target.value)}
            slotProps={{ htmlInput: { inputMode: "numeric" } }}
          />
          {!editing && (
            <TextField
              label="Пароль"
              type="password"
              required
              size="small"
              value={password}
              disabled={busy}
              onChange={(e) => setPassword(e.target.value)}
              autoComplete="new-password"
            />
          )}
          <FormGroup>
            <FormLabel component="legend">Роли</FormLabel>
            {ROLES.map((r) => (
              <Tooltip key={r.id} title={isSelf && r.id === ADMIN_ROLE_ID ? SELF_HINT : ""}>
                <span>
                  <FormControlLabel
                    label={r.name}
                    control={
                      <Checkbox
                        checked={roleIds.includes(r.id)}
                        disabled={busy || (isSelf && r.id === ADMIN_ROLE_ID)}
                        onChange={() => toggleRole(r.id)}
                      />
                    }
                  />
                </span>
              </Tooltip>
            ))}
          </FormGroup>
          {errors.length > 0 && (
            <Alert severity="error">
              <Stack component="ul" sx={{ m: 0, pl: 2 }}>
                {errors.map((e) => (
                  <li key={e}>{e}</li>
                ))}
              </Stack>
            </Alert>
          )}
          {save.error && (
            <Alert severity="error">
              {toApiError(save.error).message || "Не удалось сохранить пользователя"}
            </Alert>
          )}
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={close} disabled={busy}>
          Отмена
        </Button>
        <Button variant="contained" onClick={submit} disabled={busy}>
          {editing ? "Сохранить" : "Создать"}
        </Button>
      </DialogActions>
    </Dialog>
  );
}

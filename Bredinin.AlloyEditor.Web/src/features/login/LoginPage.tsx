import { useState, type FormEvent } from "react";
import { Alert, Box, Button, Paper, TextField, Typography } from "@mui/material";
import { Navigate, useNavigate } from "react-router-dom";
import { useAuth } from "../../auth/useAuth";
import { toApiError } from "../../api/errors";

const WRONG_CREDENTIALS = "Неверный логин или пароль";
const REQUIRED = "Обязательное поле";

export function LoginPage() {
  const { status, login } = useAuth();
  const navigate = useNavigate();
  const [loginValue, setLoginValue] = useState("");
  const [password, setPassword] = useState("");
  const [touched, setTouched] = useState(false);
  const [pending, setPending] = useState(false);
  const [error, setError] = useState<string | null>(null);

  if (status === "authenticated") return <Navigate to="/grades" replace />;

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setTouched(true);
    if (!loginValue.trim() || !password) return;
    setPending(true);
    setError(null);
    try {
      await login(loginValue.trim(), password);
      navigate("/grades", { replace: true });
    } catch (err) {
      const apiError = toApiError(err);
      // The backend answers 500 for wrong credentials, so treat it like 401 here.
      setError(apiError.status === 401 || apiError.status === 500 ? WRONG_CREDENTIALS : apiError.message);
    } finally {
      setPending(false);
    }
  };

  return (
    <Box sx={{ display: "flex", justifyContent: "center", alignItems: "center", minHeight: "100vh", p: 2 }}>
      <Paper component="form" noValidate onSubmit={onSubmit} sx={{ p: 4, width: 360, display: "grid", gap: 2 }}>
        <Typography variant="h5" component="h1">
          Вход
        </Typography>
        {error && <Alert severity="error">{error}</Alert>}
        <TextField
          label="Логин"
          value={loginValue}
          onChange={(e) => setLoginValue(e.target.value)}
          error={touched && !loginValue.trim()}
          helperText={touched && !loginValue.trim() ? REQUIRED : undefined}
          autoComplete="username"
          autoFocus
        />
        <TextField
          label="Пароль"
          type="password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          error={touched && !password}
          helperText={touched && !password ? REQUIRED : undefined}
          autoComplete="current-password"
        />
        <Button type="submit" variant="contained" disabled={pending}>
          Войти
        </Button>
      </Paper>
    </Box>
  );
}

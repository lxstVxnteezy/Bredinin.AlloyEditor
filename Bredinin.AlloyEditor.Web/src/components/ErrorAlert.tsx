import { Alert } from "@mui/material";
import { toApiError } from "../api/errors";

export function ErrorAlert({ error }: { error: unknown }) {
  const message = toApiError(error).message || "Произошла ошибка";
  return <Alert severity="error">{message}</Alert>;
}

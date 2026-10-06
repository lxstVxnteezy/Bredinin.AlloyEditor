import type { ReactNode } from "react";
import { Alert } from "@mui/material";
import { useAuth } from "./useAuth";

export function RequireRole({ role, children }: { role?: "admin"; children: ReactNode }) {
  const { isAdmin } = useAuth();
  if (role === "admin" && !isAdmin) return <Alert severity="error">Недостаточно прав</Alert>;
  return <>{children}</>;
}

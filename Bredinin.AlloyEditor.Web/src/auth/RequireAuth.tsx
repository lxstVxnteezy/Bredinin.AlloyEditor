import { Box, CircularProgress } from "@mui/material";
import { Navigate, Outlet } from "react-router-dom";
import { useAuth } from "./useAuth";

export function RequireAuth() {
  const { status } = useAuth();
  if (status === "loading") {
    return (
      <Box sx={{ display: "flex", justifyContent: "center", alignItems: "center", minHeight: "100vh" }}>
        <CircularProgress />
      </Box>
    );
  }
  if (status === "anonymous") return <Navigate to="/login" replace />;
  return <Outlet />;
}

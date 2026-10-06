import { Navigate, Route, Routes } from "react-router-dom";
import { RequireAuth } from "./auth/RequireAuth";
import { RequireRole } from "./auth/RequireRole";
import { Layout } from "./components/Layout";
import { GradesPage } from "./features/alloy-grades/GradesPage";
import { SystemsPage } from "./features/alloy-systems/SystemsPage";
import { ElementsPage } from "./features/elements/ElementsPage";
import { LoginPage } from "./features/login/LoginPage";
import { UsersPage } from "./features/users/UsersPage";

export function AppRoutes() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route element={<RequireAuth />}>
        <Route element={<Layout />}>
          <Route path="/" element={<Navigate to="/grades" replace />} />
          <Route path="/grades" element={<GradesPage />} />
          <Route path="/systems" element={<SystemsPage />} />
          <Route path="/elements" element={<ElementsPage />} />
          <Route
            path="/users"
            element={
              <RequireRole role="admin">
                <UsersPage />
              </RequireRole>
            }
          />
        </Route>
      </Route>
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}

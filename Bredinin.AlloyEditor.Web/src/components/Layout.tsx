import { AppBar, Box, Button, Container, Toolbar, Typography } from "@mui/material";
import { NavLink, Outlet } from "react-router-dom";
import { useAuth } from "../auth/useAuth";

const links = [
  { to: "/grades", label: "Марки" },
  { to: "/systems", label: "Системы сплавов" },
  { to: "/elements", label: "Элементы" },
];

export function Layout() {
  const { isAdmin, userId, logout } = useAuth();
  const items = isAdmin ? [...links, { to: "/users", label: "Пользователи" }] : links;
  return (
    <Box>
      <AppBar position="static">
        <Toolbar sx={{ gap: 1 }}>
          <Typography variant="h6" sx={{ mr: 2 }}>
            AlloyEditor
          </Typography>
          {items.map((l) => (
            <Button key={l.to} color="inherit" component={NavLink} to={l.to}>
              {l.label}
            </Button>
          ))}
          <Box sx={{ flexGrow: 1 }} />
          {userId && (
            <Typography variant="body2">
              {userId} ({isAdmin ? "администратор" : "пользователь"})
            </Typography>
          )}
          <Button color="inherit" onClick={logout}>
            Выйти
          </Button>
        </Toolbar>
      </AppBar>
      <Container sx={{ py: 3 }}>
        <Outlet />
      </Container>
    </Box>
  );
}

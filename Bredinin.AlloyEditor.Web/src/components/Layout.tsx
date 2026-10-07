import { AppBar, Avatar, Box, Container, IconButton, Toolbar, Tooltip, Typography } from "@mui/material";
import LogoutIcon from "@mui/icons-material/LogoutRounded";
import { NavLink, Outlet } from "react-router-dom";
import { useAuth } from "../auth/useAuth";
import { temperGradient } from "../theme";

const links = [
  { to: "/grades", label: "Марки" },
  { to: "/systems", label: "Системы сплавов" },
  { to: "/elements", label: "Элементы" },
];

function BrandMark() {
  return (
    <Box
      component="svg"
      viewBox="0 0 32 32"
      aria-hidden
      sx={{ width: 30, height: 30, flexShrink: 0 }}
    >
      <path d="M16 2.5 28 9.25v13.5L16 29.5 4 22.75V9.25L16 2.5Z" fill="#2A4E7C" />
      <path d="M16 2.5 28 9.25 16 16 4 9.25 16 2.5Z" fill="#4C74A8" />
      <path d="M16 16v13.5L4 22.75V9.25L16 16Z" fill="#1D3A5F" />
      <rect x="13.2" y="19" width="5.6" height="2.2" rx="1.1" fill="#E3B94A" />
    </Box>
  );
}

export function Layout() {
  const { isAdmin, userId, userLogin, logout } = useAuth();
  const items = isAdmin ? [...links, { to: "/users", label: "Пользователи" }] : links;
  const roleLabel = isAdmin ? "администратор" : "пользователь";
  const displayName = userLogin || null;

  return (
    <Box sx={{ minHeight: "100vh", bgcolor: "background.default" }}>
      <AppBar
        position="sticky"
        color="inherit"
        elevation={0}
        sx={{ bgcolor: "rgba(255,255,255,0.92)", backdropFilter: "blur(8px)", color: "text.primary" }}
      >
        <Toolbar sx={{ gap: 4, minHeight: { xs: 60, md: 64 }, px: { xs: 2, md: 4 } }}>
          <Box sx={{ display: "flex", alignItems: "center", gap: 1.25 }}>
            <BrandMark />
            <Typography variant="h6" component="span" sx={{ fontSize: "1.15rem" }}>
              AlloyEditor
            </Typography>
          </Box>

          <Box component="nav" sx={{ display: "flex", alignSelf: "stretch", gap: 0.5, flexGrow: 1, overflowX: "auto" }}>
            {items.map((l) => (
              <Box
                key={l.to}
                component={NavLink}
                to={l.to}
                sx={{
                  position: "relative",
                  display: "flex",
                  alignItems: "center",
                  px: 1.75,
                  color: "text.secondary",
                  fontWeight: 500,
                  fontSize: "0.9375rem",
                  textDecoration: "none",
                  whiteSpace: "nowrap",
                  transition: "color 120ms",
                  "&:hover": { color: "text.primary" },
                  "&:focus-visible": { outline: "2px solid #2A4E7C", outlineOffset: -4, borderRadius: 1 },
                  "&.active": { color: "primary.main", fontWeight: 600 },
                  "&.active::after": {
                    content: '""',
                    position: "absolute",
                    left: 14,
                    right: 14,
                    bottom: 0,
                    height: 3,
                    borderRadius: "3px 3px 0 0",
                    bgcolor: "primary.main",
                  },
                }}
              >
                {l.label}
              </Box>
            ))}
          </Box>

          <Box sx={{ display: "flex", alignItems: "center", gap: 1.5 }}>
            {(displayName || userId) && (
              <Box sx={{ display: "flex", alignItems: "center", gap: 1.25 }}>
                <Avatar sx={{ width: 34, height: 34, bgcolor: "primary.light", color: "primary.main", fontSize: "0.9rem", fontWeight: 650 }}>
                  {(displayName ?? "?").charAt(0).toUpperCase()}
                </Avatar>
                <Box sx={{ lineHeight: 1.2, display: { xs: "none", sm: "block" } }}>
                  <Typography variant="body2" sx={{ fontWeight: 600 }}>
                    {displayName ?? roleLabel}
                  </Typography>
                  {displayName && (
                    <Typography variant="caption" color="text.secondary">
                      {roleLabel}
                    </Typography>
                  )}
                </Box>
              </Box>
            )}
            <Tooltip title="Выйти">
              <IconButton aria-label="Выйти" onClick={logout} sx={{ color: "text.secondary" }}>
                <LogoutIcon fontSize="small" />
              </IconButton>
            </Tooltip>
          </Box>
        </Toolbar>
        <Box aria-hidden sx={{ height: 3, background: temperGradient }} />
      </AppBar>

      <Container maxWidth={false} sx={{ maxWidth: 1240, py: { xs: 3, md: 5 }, px: { xs: 2, md: 4 } }}>
        <Outlet />
      </Container>
    </Box>
  );
}

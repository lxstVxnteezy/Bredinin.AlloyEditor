import { Component, type ReactNode } from "react";
import { Alert, Box, Button } from "@mui/material";

export class ErrorBoundary extends Component<{ children: ReactNode }, { failed: boolean }> {
  state = { failed: false };

  static getDerivedStateFromError() {
    return { failed: true };
  }

  render() {
    if (!this.state.failed) return this.props.children;
    return (
      <Box sx={{ p: 3 }}>
        <Alert
          severity="error"
          action={
            <Button color="inherit" size="small" onClick={() => window.location.reload()}>
              Обновить
            </Button>
          }
        >
          Что-то пошло не так. Обновите страницу.
        </Alert>
      </Box>
    );
  }
}

import { createTheme, CssBaseline, ThemeProvider, Typography } from "@mui/material";

const theme = createTheme();

export default function App() {
  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <Typography variant="h4" component="h1">
        AlloyEditor
      </Typography>
    </ThemeProvider>
  );
}

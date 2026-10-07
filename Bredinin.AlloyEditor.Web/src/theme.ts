import "@fontsource-variable/golos-text";
import { alpha, createTheme } from "@mui/material";

// Temper colours: the oxide tints steel takes on as it is heated, from straw to blue.
export const temperGradient = "linear-gradient(90deg, #E3B94A 0%, #C98B3C 28%, #8C5A8E 62%, #3F6FB0 100%)";

const ink = "#17202B";
const muted = "#5B6878";
const line = "#DCE1E8";
const steel = "#2A4E7C";

const fontFamily = '"Golos Text Variable", "Segoe UI", system-ui, sans-serif';

export const theme = createTheme({
  palette: {
    primary: { main: steel, dark: "#1D3A5F", light: "#E6EDF6", contrastText: "#fff" },
    secondary: { main: "#C98B3C" },
    error: { main: "#B83A3A" },
    background: { default: "#EEF1F4", paper: "#FFFFFF" },
    text: { primary: ink, secondary: muted },
    divider: line,
  },
  shape: { borderRadius: 10 },
  typography: {
    fontFamily,
    h4: { fontWeight: 650, letterSpacing: "-0.02em" },
    h5: { fontWeight: 650, letterSpacing: "-0.015em", fontSize: "1.65rem" },
    h6: { fontWeight: 650, letterSpacing: "-0.01em" },
    subtitle1: { fontWeight: 600 },
    body2: { fontSize: "0.875rem" },
    button: { textTransform: "none", fontWeight: 600, letterSpacing: 0 },
  },
  components: {
    MuiCssBaseline: {
      styleOverrides: {
        body: { fontFeatureSettings: '"tnum" 1, "ss01" 1' },
        "::selection": { background: alpha(steel, 0.18) },
      },
    },
    MuiButton: {
      defaultProps: { disableElevation: true },
      styleOverrides: {
        root: { borderRadius: 10, paddingInline: 16, minHeight: 40 },
        sizeSmall: { minHeight: 32 },
        outlined: { borderColor: line, color: ink, "&:hover": { borderColor: steel, backgroundColor: alpha(steel, 0.04) } },
      },
    },
    MuiIconButton: {
      styleOverrides: { root: { borderRadius: 8 } },
    },
    MuiTextField: {
      defaultProps: { size: "small" },
    },
    MuiOutlinedInput: {
      styleOverrides: {
        root: {
          borderRadius: 10,
          backgroundColor: "#fff",
          "& .MuiOutlinedInput-notchedOutline": { borderColor: line },
          "&:hover .MuiOutlinedInput-notchedOutline": { borderColor: "#B9C2CE" },
          "&.Mui-focused .MuiOutlinedInput-notchedOutline": { borderColor: steel, borderWidth: 1.5 },
        },
      },
    },
    MuiPaper: {
      defaultProps: { elevation: 0 },
      styleOverrides: {
        root: { backgroundImage: "none" },
        outlined: { borderColor: line },
      },
    },
    MuiTableContainer: {
      styleOverrides: { root: { border: `1px solid ${line}`, borderRadius: 14, backgroundColor: "#fff" } },
    },
    MuiTableCell: {
      styleOverrides: {
        root: { borderBottomColor: "#E7EBF0", paddingBlock: 14, paddingInline: 20 },
        head: {
          backgroundColor: "#F6F8FA",
          color: muted,
          fontSize: "0.8125rem",
          fontWeight: 600,
          borderBottomColor: line,
          whiteSpace: "nowrap",
        },
      },
    },
    MuiTableRow: {
      styleOverrides: {
        root: {
          "&:last-child td": { borderBottom: 0 },
          "&.MuiTableRow-hover:hover": { backgroundColor: alpha(steel, 0.045) },
          "&.Mui-selected, &.Mui-selected:hover": { backgroundColor: alpha(steel, 0.08) },
          "&:focus-visible": { outline: `2px solid ${steel}`, outlineOffset: -2 },
        },
      },
    },
    MuiTablePagination: {
      styleOverrides: {
        root: { color: muted, borderTop: 0 },
        selectLabel: { fontSize: "0.8125rem" },
        displayedRows: { fontSize: "0.8125rem" },
      },
    },
    MuiDialog: {
      styleOverrides: { paper: { borderRadius: 16, border: `1px solid ${line}` } },
    },
    MuiDialogTitle: {
      styleOverrides: { root: { fontWeight: 650, letterSpacing: "-0.01em" } },
    },
    MuiDialogActions: {
      styleOverrides: { root: { padding: "12px 24px 20px" } },
    },
    MuiDrawer: {
      styleOverrides: { paper: { borderLeft: `1px solid ${line}` } },
    },
    MuiChip: {
      styleOverrides: { root: { fontWeight: 500 } },
    },
    MuiTooltip: {
      styleOverrides: { tooltip: { backgroundColor: ink, fontSize: "0.75rem", borderRadius: 8 } },
    },
    MuiAlert: {
      styleOverrides: { root: { borderRadius: 12 } },
    },
  },
});

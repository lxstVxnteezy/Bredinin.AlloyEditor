import { createContext, useCallback, useContext, useMemo, useState, type ReactNode } from "react";
import { Alert, Snackbar, type AlertColor } from "@mui/material";

type Notify = (message: string, severity?: AlertColor) => void;

const NotifyContext = createContext<Notify | null>(null);

export function NotificationsProvider({ children }: { children: ReactNode }) {
  const [current, setCurrent] = useState<{ message: string; severity: AlertColor } | null>(null);
  const [open, setOpen] = useState(false);

  const notify = useCallback<Notify>((message, severity = "success") => {
    setCurrent({ message, severity });
    setOpen(true);
  }, []);

  return (
    <NotifyContext.Provider value={notify}>
      {children}
      <Snackbar open={open} autoHideDuration={4000} onClose={() => setOpen(false)}>
        <Alert severity={current?.severity ?? "success"} onClose={() => setOpen(false)} variant="filled">
          {current?.message}
        </Alert>
      </Snackbar>
    </NotifyContext.Provider>
  );
}

export function useNotify(): { notify: Notify } {
  const notify = useContext(NotifyContext);
  if (!notify) throw new Error("useNotify must be used within NotificationsProvider");
  return useMemo(() => ({ notify }), [notify]);
}

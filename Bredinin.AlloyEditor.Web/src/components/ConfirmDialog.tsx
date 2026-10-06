import { Button, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle } from "@mui/material";

interface Props {
  open: boolean;
  title: string;
  text: string;
  onConfirm(): void;
  onCancel(): void;
}

export function ConfirmDialog({ open, title, text, onConfirm, onCancel }: Props) {
  return (
    <Dialog open={open} onClose={onCancel}>
      <DialogTitle>{title}</DialogTitle>
      <DialogContent>
        <DialogContentText>{text}</DialogContentText>
      </DialogContent>
      <DialogActions>
        <Button onClick={onCancel}>Отмена</Button>
        <Button onClick={onConfirm} color="error" variant="contained">
          Подтвердить
        </Button>
      </DialogActions>
    </Dialog>
  );
}

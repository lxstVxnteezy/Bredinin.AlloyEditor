// Mirrors the seed data of the Identity service (migration 2025_08_25_2116_InsertValuesInTables).
// Gateway has no endpoint to list roles; replace this constant with a dictionary
// endpoint as soon as one exists.
export const ROLES = [
  { id: "1798a758-829b-4839-9180-0a205545a450", name: "Администратор" },
  { id: "7a593027-99d0-4a47-91b1-5c73049cecc1", name: "Пользователь" },
] as const;

export const ADMIN_ROLE_ID = ROLES[0].id;

export const SELF_HINT = "Нельзя применить к своей учётной записи";

export const roleName = (id: string): string => ROLES.find((r) => r.id === id)?.name ?? id;

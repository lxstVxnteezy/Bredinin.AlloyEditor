const KEY = "alloy.refreshToken";

export const getRefreshToken = (): string | null => localStorage.getItem(KEY);
export const setRefreshToken = (t: string): void => localStorage.setItem(KEY, t);
export const clearRefreshToken = (): void => localStorage.removeItem(KEY);

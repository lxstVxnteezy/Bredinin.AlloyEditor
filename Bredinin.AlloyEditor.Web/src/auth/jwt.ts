const ROLE_CLAIM = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

export interface ParsedJwt {
  userId: string | null;
  login: string | null;
  roles: string[];
}

function decodePayload(token: string): Record<string, unknown> | null {
  try {
    const part = token.split(".")[1];
    if (!part) return null;
    const b64 = part.replace(/-/g, "+").replace(/_/g, "/");
    const padded = b64 + "=".repeat((4 - (b64.length % 4)) % 4);
    const bytes = Uint8Array.from(atob(padded), (c) => c.charCodeAt(0));
    const parsed: unknown = JSON.parse(new TextDecoder().decode(bytes));
    return parsed && typeof parsed === "object" ? (parsed as Record<string, unknown>) : null;
  } catch {
    return null;
  }
}

export function parseJwt(token: string): ParsedJwt {
  const p = decodePayload(token);
  if (!p) return { userId: null, login: null, roles: [] };
  const raw = p[ROLE_CLAIM];
  const roles = Array.isArray(raw)
    ? raw.filter((r): r is string => typeof r === "string")
    : typeof raw === "string"
      ? [raw]
      : [];
  const id = p.userId ?? p.sub;
  return {
    userId: typeof id === "string" ? id : null,
    login: typeof p.userLogin === "string" ? p.userLogin : null,
    roles,
  };
}

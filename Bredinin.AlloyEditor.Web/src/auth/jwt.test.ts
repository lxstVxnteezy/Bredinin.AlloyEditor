import { describe, expect, it } from "vitest";
import { parseJwt } from "./jwt";

const ROLE = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

function makeToken(payload: object): string {
  const b64 = (o: object) => btoa(JSON.stringify(o)).replace(/\+/g, "-").replace(/\//g, "_").replace(/=+$/, "");
  return `${b64({ alg: "HS256" })}.${b64(payload)}.sig`;
}

describe("parseJwt", () => {
  it("reads a single role string", () => {
    expect(parseJwt(makeToken({ [ROLE]: "Admin", userId: "u1" }))).toEqual({ userId: "u1", login: null, roles: ["Admin"] });
  });
  it("reads a roles array", () => {
    expect(parseJwt(makeToken({ [ROLE]: ["admin", "user"] })).roles).toEqual(["admin", "user"]);
  });
  it("returns empty roles without claim and falls back to sub", () => {
    expect(parseJwt(makeToken({ sub: "s1" }))).toEqual({ userId: "s1", login: null, roles: [] });
  });
  it("reads the userLogin claim", () => {
    expect(parseJwt(makeToken({ userId: "u1", userLogin: "ivan" })).login).toBe("ivan");
  });
  it("handles garbage tokens", () => {
    expect(parseJwt("garbage")).toEqual({ userId: null, login: null, roles: [] });
  });
});

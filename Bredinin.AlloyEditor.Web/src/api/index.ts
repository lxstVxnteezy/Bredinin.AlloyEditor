import { http } from "./http";
import type {
  AlloyGrade,
  AlloySystem,
  AuthResponse,
  ChemicalElement,
  CreateAlloyGradeRequest,
  CreateUserRequest,
  EditUserRequest,
  InfoAlloyGradeByMain,
  PagedResponse,
  UpdateUserResponse,
  User,
} from "./types";

const base = "/api/gateway";

export const login = (l: string, p: string) =>
  http.post<AuthResponse>(`${base}/identity/login`, { login: l, password: p }).then((r) => r.data);

export const refreshToken = (t: string) =>
  http.post<AuthResponse>(`${base}/identity/refresh-token`, { refreshToken: t }).then((r) => r.data);

export const revokeAccess = (userId: string) =>
  http.post(`${base}/identity/${userId}/revoke-access`).then(() => undefined);

export interface GradesParams {
  search?: string;
  page: number;
  pageSize: number;
}

export const getGrades = ({ search, page, pageSize }: GradesParams) =>
  http
    .get<PagedResponse<AlloyGrade>>(`${base}/alloy-grade`, {
      params: { search: search || undefined, page, pageSize },
    })
    .then((r) => r.data);

export const createGrade = (req: CreateAlloyGradeRequest) =>
  http.post<string>(`${base}/alloy-grade`, req).then((r) => r.data);

export const deleteGrade = (id: string) =>
  http.delete(`${base}/alloy-grade/${id}`).then(() => undefined);

export const getGradesForMainElement = (id: string) =>
  http.get<InfoAlloyGradeByMain[]>(`${base}/alloy-grade/for-main-element/${id}`).then((r) => r.data);

export const getSystems = () => http.get<AlloySystem[]>(`${base}/alloy-system`).then((r) => r.data);

export const getElements = () =>
  http.get<ChemicalElement[]>(`${base}/dictionary/chemical-elements`).then((r) => r.data);

export const getUsers = () => http.get<User[]>(`${base}/admin/search-users`).then((r) => r.data);

export const createUser = (req: CreateUserRequest) =>
  http.post<string>(`${base}/admin/user`, req).then((r) => r.data);

export const updateUser = (req: EditUserRequest) =>
  http.put<UpdateUserResponse>(`${base}/admin/user`, req).then((r) => r.data);

export const deleteUser = (id: string) =>
  http.delete(`${base}/admin/${id}`).then(() => undefined);

export * from "./types";
export { ApiError, toApiError } from "./errors";
export { http, setAuthHandlers } from "./http";

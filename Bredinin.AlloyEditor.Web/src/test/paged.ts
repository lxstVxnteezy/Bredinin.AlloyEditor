import { HttpResponse } from "msw";

// Mimics GET /api/gateway/alloy-grade: server-side search by name plus paging.
export function pagedGrades<T extends { name: string }>(all: T[], request: Request) {
  const params = new URL(request.url).searchParams;
  const search = (params.get("search") ?? "").toLowerCase();
  const page = Number(params.get("page") ?? 1);
  const pageSize = Number(params.get("pageSize") ?? 20);
  const matched = all.filter((g) => g.name.toLowerCase().includes(search));
  return HttpResponse.json({
    items: matched.slice((page - 1) * pageSize, page * pageSize),
    totalCount: matched.length,
    page,
    pageSize,
    totalPages: Math.ceil(matched.length / pageSize),
  });
}

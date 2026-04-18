namespace Bredinin.AlloyEditor.Contracts.Common;

public record PagedResponse<T>(
    T[] Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);
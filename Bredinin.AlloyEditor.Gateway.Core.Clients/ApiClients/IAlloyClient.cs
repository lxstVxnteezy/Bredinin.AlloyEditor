using Bredinin.AlloyEditor.Contracts.Common;
using Bredinin.AlloyEditor.Contracts.Common.AlloyGrade;
using Microsoft.AspNetCore.Mvc;
using Refit;

namespace Bredinin.AlloyEditor.Gateway.Core.Clients.ApiClients;

public interface IAlloyClient
{
    [Get("/api/alloys")]
    Task<PagedResponse<AlloyGradeResponse>> GetGrades(
        [Query] string? search = null,
        [Query] int page = 1,
        [Query] int pageSize = 20,
        CancellationToken ctn = default);

    [Post("/api/alloys")]
    Task<Guid> CreateAlloyGrade(CreateAlloyGradeRequest request, CancellationToken ctn);

    [Delete("/api/alloys/{id}")]
    Task<IActionResult> DeleteAlloyGrade(
        [AliasAs("id")] Guid id,
        CancellationToken ctn = default);

    [Get("/api/alloys/for-main-element/{id}")]
    Task<InfoAlloyGradeByMainResponse[]> GetAlloysForMainElement(
        [AliasAs("id")] Guid id,
        CancellationToken ctn = default);

}
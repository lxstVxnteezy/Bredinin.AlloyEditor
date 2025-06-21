using Bredinin.AlloyEditor.Contracts.Common.AlloyGrade;
using Refit;

namespace Bredinin.AlloyEditor.Common.Desktop.Api.Alloys
{
    public interface IAlloyApiService
    {
        [Get("/api/alloys")]
        Task<AlloyGradeResponse[]> GetGradesAsync(CancellationToken ctn = default);

        [Post("/api/alloys")]
        Task<Guid> CreateAlloyGradeAsync(
            [Body] CreateAlloyGradeRequest request,
            CancellationToken ctn = default);

        [Delete("/api/alloys/{id}")]
        Task DeleteAlloyGradeAsync(
            Guid id,
            CancellationToken ctn = default);

        [Get("/api/alloys/for-main-element/{id}")]
        Task<InfoAlloyGradeByMainResponse[]> GetAlloysForMainElementAsync(
            Guid id,
            CancellationToken ctn = default);
    }
}

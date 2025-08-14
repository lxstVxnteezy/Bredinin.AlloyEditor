using Bredinin.AlloyEditor.Contracts.Common.AlloyGrade;
using Bredinin.AlloyEditor.DAL;
using Bredinin.AlloyEditor.Handlers.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.AlloyEditor.Handlers.Methods.Alloy.AlloyGrade
{
    public interface ISearchAlloyGradeHandler : IHandler
    {
        public Task<AlloyGradeResponse[]> Handle(CancellationToken ctn);
    }

    internal class SearchAlloyGradeHandler(ServiceDbContext context)
        : ISearchAlloyGradeHandler
    {
        public async Task<AlloyGradeResponse[]> Handle(CancellationToken ctn)
        {
            return await context.AlloyGrades
                .AsNoTracking()
                .Include(x => x.ChemicalCompositions)
                .Select(x => new AlloyGradeResponse(
                    x.Id,
                    x.Name,
                    x.Description,
                    x.AlloySystemId,
                    x.ChemicalCompositions
                        .Select(ChemicalCompositionHelper.Convert)
                        .ToArray()
                ))
                .ToArrayAsync(ctn);
        }
    }
}

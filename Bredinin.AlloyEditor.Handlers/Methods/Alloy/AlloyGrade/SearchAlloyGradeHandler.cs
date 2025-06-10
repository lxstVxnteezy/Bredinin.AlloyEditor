using Bredinin.AlloyEditor.Contracts.Common.AlloyGrade;
using Bredinin.AlloyEditor.DAL.Core;
using Bredinin.AlloyEditor.Handlers.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.AlloyEditor.Handlers.Methods.Alloy.AlloyGrade
{
    public interface ISearchAlloyGradeHandler : IHandler
    {
        public Task<AlloyGradeResponse[]> Handle(CancellationToken ctn);
    }

    internal class SearchAlloyGradeHandler(IRepository<Domain.Alloys.AlloyGrade> alloyGradeRepository)
        : ISearchAlloyGradeHandler
    {
        public async Task<AlloyGradeResponse[]> Handle(CancellationToken ctn)
        {
            var alloys = await alloyGradeRepository.Query
                .Include(x => x.ChemicalCompositions)
                .ToArrayAsync(ctn);

            return alloys.Select(MapToResponse).ToArray();
        }

        private static AlloyGradeResponse MapToResponse(Domain.Alloys.AlloyGrade alloyGrade)
        {
            return new AlloyGradeResponse(
                Id: alloyGrade.Id,
                Name: alloyGrade.Name,
                Description: alloyGrade.Description,
                AlloySystemId: alloyGrade.AlloySystemId,
                ChemicalCompositions: alloyGrade.ChemicalCompositions
                    .Select(ChemicalCompositionHelper.Convert)
                    .ToArray()
            );
        }
    }
}

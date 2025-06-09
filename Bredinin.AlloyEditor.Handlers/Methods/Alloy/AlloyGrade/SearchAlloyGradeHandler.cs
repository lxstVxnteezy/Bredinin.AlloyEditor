using Bredinin.AlloyEditor.Contracts.Common.AlloyGrade;
using Bredinin.AlloyEditor.Contracts.Common.ChemicalCompositions;
using Bredinin.AlloyEditor.DAL.Core;
using Bredinin.MyPetProject.Domain.Alloys;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.AlloyEditor.Handlers.Methods.Alloy.AlloyGrade
{
    public interface ISearchAlloyGradeHandler : IHandler
    {
        public Task<AlloyGradeResponse[]> Handle(CancellationToken ctn);
    }

    internal class SearchAlloyGradeHandler(IRepository<MyPetProject.Domain.Alloys.AlloyGrade> alloyGradeRepository)
        : ISearchAlloyGradeHandler
    {
        public async Task<AlloyGradeResponse[]> Handle(CancellationToken ctn)
        {
            var alloys = await alloyGradeRepository.Query
                .Include(x => x.ChemicalCompositions)
                .ToArrayAsync(ctn);

            return alloys.Select(MapToResponse).ToArray();
        }

        private static AlloyGradeResponse MapToResponse(MyPetProject.Domain.Alloys.AlloyGrade alloyGrade)
        {
            return new AlloyGradeResponse(
                Id: alloyGrade.Id,
                Name: alloyGrade.Name,
                Description: alloyGrade.Description,
                AlloySystemId: alloyGrade.AlloySystemId,
                ChemicalCompositions: alloyGrade.ChemicalCompositions.Select(MapToCompositions).ToArray()
            );
        }

        private static ChemicalCompositionsResponse MapToCompositions(AlloyChemicalCompositions compositionsResponse)
        {
            return new ChemicalCompositionsResponse(
                Id: compositionsResponse.Id,
                MinValue: compositionsResponse.MinValue,
                MaxValue: compositionsResponse.MaxValue,
                ExactValue: compositionsResponse.ExactValue,
                ChemicalElementId: compositionsResponse.ChemicalElementId
            );
        }
    }
}

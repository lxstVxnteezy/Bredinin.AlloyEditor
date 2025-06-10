using Bredinin.AlloyEditor.Contracts.Common.AlloyGrade;
using Bredinin.AlloyEditor.Core.Http.Exceptions;
using Bredinin.AlloyEditor.DAL.Core;
using Bredinin.AlloyEditor.Handlers.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.AlloyEditor.Handlers.Methods.Alloy.AlloyGrade
{
    public interface IGetAlloysByMainElementHandler : IHandler
    {
        Task<InfoAlloyGradeByMainResponse[]> Handle(Guid id, CancellationToken ctn);
    }

    internal class GetAlloysByMainElementHandler(
        IRepository<Domain.Alloys.AlloyGrade> alloyGradeRepository)
        : IGetAlloysByMainElementHandler
    {
        public async Task<InfoAlloyGradeByMainResponse[]> Handle(Guid id, CancellationToken ctn)
        {
            var result = await alloyGradeRepository.Query.Where(x => x.AlloySystemId == id)
                .Include(x=>x.ChemicalCompositions)
                .ToArrayAsync(ctn);

            if (result.Length == 0)
                throw new BusinessException("Alloys this system not found in db");

            return result.Select(MapToResponse).ToArray();
        }

        private InfoAlloyGradeByMainResponse MapToResponse(Domain.Alloys.AlloyGrade alloyGrade)
        {
            return new InfoAlloyGradeByMainResponse(
                Id: alloyGrade.Id,
                Name: alloyGrade.Name,
                Description: alloyGrade.Description,
                ChemicalCompositions: alloyGrade.ChemicalCompositions
                    .Select(ChemicalCompositionConvertToResponse.Convert)
                    .ToArray());
        }

    }
}

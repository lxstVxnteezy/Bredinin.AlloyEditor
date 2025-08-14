using Bredinin.AlloyEditor.Contracts.Common.AlloyGrade;
using Bredinin.AlloyEditor.DAL;
using Bredinin.AlloyEditor.Handlers.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.AlloyEditor.Handlers.Methods.Alloy.AlloyGrade
{
    public interface IGetAlloysByMainElementHandler : IHandler
    {
        Task<InfoAlloyGradeByMainResponse[]> Handle(Guid id, CancellationToken ctn);
    }

    internal class GetAlloysByMainElementHandler(
        ServiceDbContext context)
        : IGetAlloysByMainElementHandler
    {
        public async Task<InfoAlloyGradeByMainResponse[]> Handle(Guid id, CancellationToken ctn)
        {
            return await context.AlloyGrades
                .AsNoTracking()
                .Where(x => x.AlloySystemId == id)
                .Include(x => x.ChemicalCompositions)
                .Select(x => new InfoAlloyGradeByMainResponse(
                    x.Id,
                    x.Name,
                    x.Description,
                    x.ChemicalCompositions
                        .Select(ChemicalCompositionHelper.Convert)
                        .ToArray()))
                .ToArrayAsync(ctn);
        }
    }
}

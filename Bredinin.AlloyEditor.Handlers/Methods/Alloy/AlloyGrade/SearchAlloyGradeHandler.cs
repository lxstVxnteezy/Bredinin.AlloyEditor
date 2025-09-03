using Bredinin.AlloyEditor.Contracts.Common.AlloyGrade;
using Bredinin.AlloyEditor.Core.Http.Exceptions;
using Bredinin.AlloyEditor.DAL;
using Bredinin.AlloyEditor.Handlers.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.AlloyEditor.Handlers.Methods.Alloy.AlloyGrade;

public interface ISearchAlloyGradeHandler : IHandler
{ 
    Task<AlloyGradeResponse[]> Handle(CancellationToken ctn);
}

internal class SearchAlloyGradeHandler(ServiceDbContext context)
    : ISearchAlloyGradeHandler
{
    public async Task<AlloyGradeResponse[]> Handle(CancellationToken ctn)
    {
        var response = await context.AlloyGrades
            .Select(x => new AlloyGradeResponse(
                x.Id,
                x.Name,
                x.Description,
                x.AlloySystemId,
                x.ChemicalCompositions
                    .AsQueryable() 
                    .Select(ChemicalCompositionHelper.ConvertExpression())
                    .ToArray()
            ))
            .ToArrayAsync(ctn);

        if (response.Length == 0)
            throw new BusinessException("alloys could not be found");
        
        return response;
    }
}
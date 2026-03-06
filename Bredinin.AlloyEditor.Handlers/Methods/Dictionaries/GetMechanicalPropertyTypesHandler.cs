using Bredinin.AlloyEditor.Contracts.Common.Dictionaries;
using Bredinin.AlloyEditor.DAL;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.AlloyEditor.Handlers.Methods.Dictionaries;

public interface IGetMechanicalPropertyTypesHandler : IHandler
{
    Task<MechanicalPropertyTypeDto[]> Handle(CancellationToken ctn);
}

internal sealed class GetMechanicalPropertyTypesHandler(ServiceDbContext context) : IGetMechanicalPropertyTypesHandler
{
    public async Task<MechanicalPropertyTypeDto[]> Handle(CancellationToken ctn)
    {
        return await context.DictMechanicalPropertyTypes
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new MechanicalPropertyTypeDto(
                x.Id,
                x.Name,
                x.Unit,
                x.Symbol,
                x.Description,
                x.ValueType,
                x.MinPossible,
                x.MaxPossible
            ))
            .ToArrayAsync(ctn);
    }
}
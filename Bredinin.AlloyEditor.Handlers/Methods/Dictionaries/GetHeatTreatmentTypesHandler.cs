using Bredinin.AlloyEditor.Contracts.Common.HeatTreatment;
using Bredinin.AlloyEditor.DAL;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.AlloyEditor.Handlers.Methods.Dictionaries;

public interface IGetHeatTreatmentTypesHandler : IHandler
{
    Task<HeatTreatmentTypeDto[]> Handle(CancellationToken ctn);
}

internal sealed class GetHeatTreatmentTypesHandler(ServiceDbContext context) : IGetHeatTreatmentTypesHandler
{
    public async Task<HeatTreatmentTypeDto[]> Handle(CancellationToken ctn)
    {
        return await context.DictTypesOfHeatTreatments
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new HeatTreatmentTypeDto(
                x.Id,
                x.Name,
                x.Description,
                x.Code,
                x.DefaultTemperatureMin,
                x.DefaultTemperatureMax,
                x.DefaultCoolingMedium
            ))
            .ToArrayAsync(ctn);
    }
}
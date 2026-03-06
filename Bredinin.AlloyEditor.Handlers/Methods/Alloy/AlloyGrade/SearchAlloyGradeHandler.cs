using Bredinin.AlloyEditor.Contracts.Common.AlloyGrade;
using Bredinin.AlloyEditor.Contracts.Common.ChemicalCompositions;
using Bredinin.AlloyEditor.Contracts.Common.HeatTreatment;
using Bredinin.AlloyEditor.Contracts.Common.MechenicalProperties;
using Bredinin.AlloyEditor.DAL;
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
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new AlloyGradeResponse(
                x.Id,
                x.Name,
                x.Description,
                x.AlloySystemId,
                x.ChemicalCompositions.Select(cc => new ChemicalCompositionsDto(
                    cc.Id,
                    cc.MinValue,
                    cc.MaxValue,
                    cc.ExactValue,
                    cc.ChemicalElementId
                )).ToArray(),
                x.HeatTreatments.Select(ht => new HeatTreatmentDto(
                    ht.Id,
                    ht.HeatTreatmentTypeId,
                    ht.HeatTreatmentType.Name,
                    ht.TemperatureMin,
                    ht.TemperatureMax,
                    ht.TemperatureExact,
                    ht.HoldingTimeMin,
                    ht.HoldingTimeMax,
                    ht.HoldingTimeExact,
                    ht.CoolingMedium,
                    ht.Description,
                    ht.StepOrder,
                    ht.IsDefault
                )).ToArray(),
                x.MechanicalProperties.Select(mp => new MechanicalPropertyDto(
                    mp.Id,
                    mp.PropertyTypeId,
                    mp.PropertyType.Name,
                    mp.PropertyType.Unit,
                    mp.PropertyType.Symbol,
                    mp.ValueMin,
                    mp.ValueMax,
                    mp.ValueExact,
                    mp.Condition,
                    mp.Source,
                    mp.HeatTreatmentId
                )).ToArray()
            ))
            .ToArrayAsync(ctn);

        return response.Length == 0 ? Array.Empty<AlloyGradeResponse>() : response;
    }
}
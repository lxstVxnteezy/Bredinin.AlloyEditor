using Bredinin.AlloyEditor.Common.Http;
using Bredinin.AlloyEditor.Contracts.Common.AlloyGrade;
using Bredinin.AlloyEditor.Contracts.Common.ChemicalCompositions;
using Bredinin.AlloyEditor.Contracts.Common.HeatTreatment;
using Bredinin.AlloyEditor.Contracts.Common.MechenicalProperties;
using Bredinin.AlloyEditor.DAL;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.AlloyEditor.Handlers.Methods.Alloy.AlloyGrade;

public interface IGetAlloyGradeByIdHandler : IHandler
{
    Task<AlloyGradeResponse> Handle(Guid id, CancellationToken ctn);
}

internal sealed class GetAlloyGradeByIdHandler(ServiceDbContext context) : IGetAlloyGradeByIdHandler
{
    public async Task<AlloyGradeResponse> Handle(Guid id, CancellationToken ctn)
    {
        var alloy = await context.AlloyGrades
            .AsNoTracking()
            .Include(x => x.ChemicalCompositions)
            .Include(x => x.HeatTreatments)
                .ThenInclude(ht => ht.HeatTreatmentType)
            .Include(x => x.MechanicalProperties)
                .ThenInclude(mp => mp.PropertyType)
            .FirstOrDefaultAsync(x => x.Id == id, ctn);

        if (alloy == null)
            throw new BusinessException($"Alloy grade not found: {id}");

        return new AlloyGradeResponse(
            alloy.Id,
            alloy.Name,
            alloy.Description,
            alloy.AlloySystemId,
            alloy.ChemicalCompositions.Select(cc => new ChemicalCompositionsDto(
                cc.Id,
                cc.MinValue,
                cc.MaxValue,
                cc.ExactValue,
                cc.ChemicalElementId
            )).ToArray(),
            alloy.HeatTreatments.Select(ht => new HeatTreatmentDto(
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
            alloy.MechanicalProperties.Select(mp => new MechanicalPropertyDto(
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
        );
    }
}
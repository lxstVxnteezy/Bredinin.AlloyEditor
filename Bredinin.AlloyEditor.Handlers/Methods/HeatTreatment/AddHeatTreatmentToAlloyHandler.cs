using Bredinin.AlloyEditor.Common.Http;
using Bredinin.AlloyEditor.Contracts.Common.HeatTreatment;
using Bredinin.AlloyEditor.DAL;
using Bredinin.AlloyEditor.Domain.Alloys;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.AlloyEditor.Handlers.Methods.HeatTreatment;

public interface IAddHeatTreatmentToAlloyHandler : IHandler
{
    Task<Guid> Handle(Guid alloyId, CreateHeatTreatmentRequest request, CancellationToken ctn);
}

internal class AddHeatTreatmentToAlloyHandler(
    ServiceDbContext context) : IAddHeatTreatmentToAlloyHandler
{
    public async Task<Guid> Handle(Guid alloyId, CreateHeatTreatmentRequest request, CancellationToken ctn)
    {
        var alloy = await context.AlloyGrades
            .Include(x => x.HeatTreatments)
            .FirstOrDefaultAsync(x => x.Id == alloyId, ctn);

        if (alloy == null)
            throw new BusinessException($"Alloy grade not found: {alloyId}");

        var typeExists = await context.DictTypesOfHeatTreatments
            .AnyAsync(x => x.Id == request.HeatTreatmentTypeId, ctn);

        if (!typeExists)
            throw new BusinessException($"Heat treatment type not found: {request.HeatTreatmentTypeId}");

        var heatTreatment = new AlloyHeatTreatment
        {
            Id = Guid.NewGuid(),
            AlloyGradeId = alloyId,
            HeatTreatmentTypeId = request.HeatTreatmentTypeId,
            TemperatureMin = request.TemperatureMin,
            TemperatureMax = request.TemperatureMax,
            TemperatureExact = request.TemperatureExact,
            HoldingTimeMin = request.HoldingTimeMin,
            HoldingTimeMax = request.HoldingTimeMax,
            HoldingTimeExact = request.HoldingTimeExact,
            CoolingMedium = request.CoolingMedium,
            Description = request.Description,
            StepOrder = request.StepOrder,
            IsDefault = request.IsDefault
        };

        await context.AlloyHeatTreatments.AddAsync(heatTreatment, ctn);
        await context.SaveChangesAsync(ctn);

        return heatTreatment.Id;
    }
}
using Bredinin.AlloyEditor.Contracts.Common.HeatTreatment;
using Bredinin.AlloyEditor.Core.Http.Exceptions;
using Bredinin.AlloyEditor.DAL;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.AlloyEditor.Handlers.Methods.HeatTreatment;

public interface IUpdateHeatTreatmentHandler : IHandler
{
    Task Handle(Guid id, UpdateHeatTreatmentRequest request, CancellationToken ctn);
}

internal sealed class UpdateHeatTreatmentHandler(ServiceDbContext context) : IUpdateHeatTreatmentHandler
{
    public async Task Handle(Guid id, UpdateHeatTreatmentRequest request, CancellationToken ctn)
    {
        var treatment = await context.AlloyHeatTreatments
            .FirstOrDefaultAsync(x => x.Id == id, ctn);

        if (treatment == null)
            throw new BusinessException($"Heat treatment not found: {id}");

        if (request.HeatTreatmentTypeId.HasValue)
        {
            var typeExists = await context.DictTypesOfHeatTreatments
                .AnyAsync(x => x.Id == request.HeatTreatmentTypeId, ctn);

            if (!typeExists)
                throw new BusinessException($"Heat treatment type not found: {request.HeatTreatmentTypeId}");

            treatment.HeatTreatmentTypeId = request.HeatTreatmentTypeId.Value;
        }

        treatment.TemperatureMin = request.TemperatureMin ?? treatment.TemperatureMin;
        treatment.TemperatureMax = request.TemperatureMax ?? treatment.TemperatureMax;
        treatment.TemperatureExact = request.TemperatureExact ?? treatment.TemperatureExact;
        treatment.HoldingTimeMin = request.HoldingTimeMin ?? treatment.HoldingTimeMin;
        treatment.HoldingTimeMax = request.HoldingTimeMax ?? treatment.HoldingTimeMax;
        treatment.HoldingTimeExact = request.HoldingTimeExact ?? treatment.HoldingTimeExact;
        treatment.CoolingMedium = request.CoolingMedium ?? treatment.CoolingMedium;
        treatment.Description = request.Description ?? treatment.Description;
        treatment.StepOrder = request.StepOrder ?? treatment.StepOrder;
        treatment.IsDefault = request.IsDefault ?? treatment.IsDefault;

        await context.SaveChangesAsync(ctn);
    }
}
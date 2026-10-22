using Bredinin.AlloyEditor.Common.Http;
using Bredinin.AlloyEditor.Contracts.Common.MechenicalProperties;
using Bredinin.AlloyEditor.DAL;
using Bredinin.AlloyEditor.Domain.Alloys;
using Bredinin.AlloyEditor.Handlers.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.AlloyEditor.Handlers.Methods.Alloy.MechanicalProperties;

public interface IAddMechanicalPropertyToAlloyHandler : IHandler
{
    Task<Guid> Handle(Guid alloyId, CreateMechanicalPropertyRequest request, CancellationToken ctn);
}

internal class AddMechanicalPropertyToAlloyHandler(
    ServiceDbContext context) : IAddMechanicalPropertyToAlloyHandler
{
    public async Task<Guid> Handle(Guid alloyId, CreateMechanicalPropertyRequest request, CancellationToken ctn)
    {
        var alloy = await context.AlloyGrades
            .FirstOrDefaultAsync(x => x.Id == alloyId, ctn);

        if (alloy == null)
            throw new BusinessException($"Alloy grade not found: {alloyId}");

        var propertyType = await context.DictMechanicalPropertyTypes
            .FirstOrDefaultAsync(x => x.Id == request.PropertyTypeId, ctn);

        if (propertyType == null)
            throw new BusinessException($"Property type not found: {request.PropertyTypeId}");

        if (request.HeatTreatmentId.HasValue)
        {
            var treatment = await context.AlloyHeatTreatments
                .AnyAsync(x => x.Id == request.HeatTreatmentId && x.AlloyGradeId == alloyId, ctn);

            if (!treatment)
                throw new BusinessException($"Heat treatment not found or doesn't belong to this alloy");
        }

        // Валидация на основе типа свойства
        MechanicalPropertyValueChecker.Assert(propertyType, request.ValueMin, request.ValueMax, request.ValueExact);

        var property = new AlloyMechanicalProperty
        {
            Id = Guid.NewGuid(),
            AlloyGradeId = alloyId,
            PropertyTypeId = request.PropertyTypeId,
            HeatTreatmentId = request.HeatTreatmentId,
            ValueMin = request.ValueMin,
            ValueMax = request.ValueMax,
            ValueExact = request.ValueExact,
            Condition = request.Condition,
            Source = request.Source,
            Notes = request.Notes
        };

        await context.AlloyMechanicalProperties.AddAsync(property, ctn);
        await context.SaveChangesAsync(ctn);

        return property.Id;
    }
}
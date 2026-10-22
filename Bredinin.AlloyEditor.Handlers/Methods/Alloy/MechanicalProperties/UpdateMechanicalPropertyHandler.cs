using Bredinin.AlloyEditor.Common.Http;
using Bredinin.AlloyEditor.Contracts.Common.MechenicalProperties;
using Bredinin.AlloyEditor.DAL;
using Bredinin.AlloyEditor.Handlers.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.AlloyEditor.Handlers.Methods.Alloy.MechanicalProperties;

public interface IUpdateMechanicalPropertyHandler : IHandler
{
    Task Handle(Guid id, UpdateMechanicalPropertyRequest request, CancellationToken ctn);
}

internal sealed class UpdateMechanicalPropertyHandler(ServiceDbContext context) : IUpdateMechanicalPropertyHandler
{
    public async Task Handle(Guid id, UpdateMechanicalPropertyRequest request, CancellationToken ctn)
    {
        var property = await context.AlloyMechanicalProperties
            .Include(x => x.PropertyType)
            .FirstOrDefaultAsync(x => x.Id == id, ctn);

        if (property == null)
            throw new BusinessException($"Mechanical property not found: {id}");

        if (request.PropertyTypeId.HasValue)
        {
            var typeExists = await context.DictMechanicalPropertyTypes
                .AnyAsync(x => x.Id == request.PropertyTypeId, ctn);

            if (!typeExists)
                throw new BusinessException($"Property type not found: {request.PropertyTypeId}");

            property.PropertyTypeId = request.PropertyTypeId.Value;
        }

        // Если меняем тип свойства — проверим валидацию
        if (request.PropertyTypeId.HasValue ||
            request.ValueMin != null ||
            request.ValueMax != null ||
            request.ValueExact != null)
        {
            var type = await context.DictMechanicalPropertyTypes
                .FirstAsync(x => x.Id == property.PropertyTypeId, ctn);

            MechanicalPropertyValueChecker.Assert(type,
                request.ValueMin ?? property.ValueMin,
                request.ValueMax ?? property.ValueMax,
                request.ValueExact ?? property.ValueExact);
        }

        property.ValueMin = request.ValueMin ?? property.ValueMin;
        property.ValueMax = request.ValueMax ?? property.ValueMax;
        property.ValueExact = request.ValueExact ?? property.ValueExact;
        property.Condition = request.Condition ?? property.Condition;
        property.Source = request.Source ?? property.Source;
        property.Notes = request.Notes ?? property.Notes;

        await context.SaveChangesAsync(ctn);
    }
}
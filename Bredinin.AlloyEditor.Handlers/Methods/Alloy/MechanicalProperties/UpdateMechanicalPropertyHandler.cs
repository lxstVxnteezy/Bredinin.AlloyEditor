using Bredinin.AlloyEditor.Common.Http;
using Bredinin.AlloyEditor.Contracts.Common.MechenicalProperties;
using Bredinin.AlloyEditor.DAL;
using Bredinin.AlloyEditor.Domain.Dictionaries;
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

            if (type.ValueType == PropertyValueType.Exact &&
                (request.ValueExact ?? property.ValueExact) == null)
                throw new BusinessException($"Exact value required for property type {type.Name}");

            if (type.ValueType == PropertyValueType.Range &&
                ((request.ValueMin ?? property.ValueMin) == null ||
                 (request.ValueMax ?? property.ValueMax) == null))
                throw new BusinessException($"Min and Max values required for property type {type.Name}");
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
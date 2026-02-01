using Bredinin.AlloyEditor.Contracts.Common.AlloyGrade;
using Bredinin.AlloyEditor.Core.Http.Exceptions;
using Bredinin.AlloyEditor.DAL;
using Bredinin.AlloyEditor.Domain.Alloys;
using Bredinin.AlloyEditor.Handlers.Validators;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.AlloyEditor.Handlers.Methods.Alloy.AlloyGrade;

public interface ICreateAlloyGradeHandler : IHandler
{
    Task<Guid> Handle(CreateAlloyGradeRequest request, CancellationToken ctn);
}

internal sealed class CreateAlloyGradeHandler(ServiceDbContext context) : ICreateAlloyGradeHandler
{
    public async Task<Guid> Handle(CreateAlloyGradeRequest request, CancellationToken ctn)
    {
        ArgumentNullException.ThrowIfNull(request);

        var alloySystemExists = await context.AlloySystems
            .AsNoTracking()
            .AnyAsync(x => x.Id == request.AlloySystemId, ctn);

        if (!alloySystemExists)
            throw new BusinessException($"Alloy system not found: {request.AlloySystemId}");

        var alloyExists = await context.AlloyGrades
            .AsNoTracking()
            .AnyAsync(x => x.Name == request.Name, ctn);

        if (alloyExists)
            throw new BusinessException($"Alloy grade '{request.Name}' already exists");

        var elementIds = request.ChemicalCompositions
            .Select(x => x.ChemicalElementId)
            .Distinct()
            .ToArray();

        var existingElements = await context.DictChemicalElements
            .AsNoTracking()
            .Where(x => elementIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync(ctn);

        var missingElements = elementIds.Except(existingElements).ToArray();

        if (missingElements.Length > 0)
            throw new BusinessException(
                $"Chemical elements not found: {string.Join(", ", missingElements)}");

        var chemicalCompositions = request.ChemicalCompositions
            .Select(MapToEntity)
            .ToList();

        var newAlloyGrade = new Domain.Alloys.AlloyGrade
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            AlloySystemId = request.AlloySystemId,
            ChemicalCompositions = chemicalCompositions
        };

        AlloyChemicalCompositionValidator
            .ValidateTotalRange(newAlloyGrade.ChemicalCompositions);

        context.AlloyGrades.Add(newAlloyGrade);

        await context.SaveChangesAsync(ctn);

        return newAlloyGrade.Id;
    }

    private static AlloyChemicalCompositions MapToEntity(
        CreateChemicalCompositionRequest request)
    {
        return new AlloyChemicalCompositions
        {
            Id = Guid.NewGuid(),
            MinValue = request.MinValue,
            MaxValue = request.MaxValue,
            ExactValue = request.ExactValue,
            ChemicalElementId = request.ChemicalElementId
        };
    }
}
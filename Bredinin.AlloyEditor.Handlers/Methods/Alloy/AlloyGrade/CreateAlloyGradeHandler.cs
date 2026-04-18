using Bredinin.AlloyEditor.Common.Http;
using Bredinin.AlloyEditor.Contracts.Common.AlloyGrade;
using Bredinin.AlloyEditor.DAL;
using Bredinin.AlloyEditor.Domain.Alloys;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.AlloyEditor.Handlers.Methods.Alloy.AlloyGrade;

public interface ICreateAlloyGradeHandler : IHandler
{
    Task<Guid> Handle(CreateAlloyGradeRequest request, CancellationToken ctn);
}

internal class CreateAlloyGradeHandler(ServiceDbContext context) : ICreateAlloyGradeHandler
{
    public async Task<Guid> Handle(CreateAlloyGradeRequest request, CancellationToken ctn)
    {
        var alloySystem = await context.AlloySystems
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.Id == request.AlloySystemId, ctn);

        if (alloySystem == null)
            throw new BusinessException($"Alloy system not found: {request.AlloySystemId}");

        var baseElementExist = request.ChemicalCompositions
           .Any(e => e.ChemicalElementId == alloySystem.BaseElementId);

        if (!baseElementExist)
            throw new BusinessException($"Base element for alloy creation is not added. '{request.Name}");

        ArgumentNullException.ThrowIfNull(request);

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
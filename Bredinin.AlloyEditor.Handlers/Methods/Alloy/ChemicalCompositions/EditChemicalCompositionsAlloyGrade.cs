using Bredinin.AlloyEditor.Contracts.Common.ChemicalCompositions;
using Bredinin.AlloyEditor.Core.Http.Exceptions;
using Bredinin.AlloyEditor.DAL;
using Bredinin.AlloyEditor.Domain.Alloys;
using Bredinin.AlloyEditor.Handlers.Validators;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.AlloyEditor.Handlers.Methods.Alloy.ChemicalCompositions
{
    public interface IEditChemicalCompositionsAlloyGradeHandler : IHandler
    {
        Task<ChemicalCompositionsDto[]> Handle(Guid alloyGradeId,
            ChemicalCompositionsRequest[] request,
            CancellationToken ctn);
    }
    public class EditChemicalCompositionsAlloyGradeHandler
        (ServiceDbContext context) : IEditChemicalCompositionsAlloyGradeHandler
    {
        public async Task<ChemicalCompositionsDto[]> Handle(Guid alloyGradeId, ChemicalCompositionsRequest[] request, CancellationToken ctn)
        {
            var requestedElementIds = request.Select(r => r.ChemicalElementId)
                .Distinct()
                .ToArray();

            var existingIds = await context.DictChemicalElements
                .Where(x => requestedElementIds.Contains(x.Id))
                .Select(x => x.Id)
                .ToArrayAsync(cancellationToken: ctn);

            var missing = requestedElementIds.Except(existingIds).ToArray();
          
            if (missing.Length > 0)
                throw new BusinessException($"ChemicalElement(s) not found in db: {string.Join(',', missing)}");

            var foundAlloy = await context.AlloyGrades
                .Include(x => x.ChemicalCompositions)
                .SingleOrDefaultAsync(x => x.Id == alloyGradeId, ctn);

            if (foundAlloy == null)
                throw new BusinessException($"Alloy with {alloyGradeId} not found in db");

            var chemicalCompositions = request
                .Select(rc => MapToEntity(rc, foundAlloy.Id))
                .ToArray();

            AlloyChemicalCompositionValidator.ValidateTotalRange(chemicalCompositions);

            foundAlloy.ChemicalCompositions.Clear();

            foreach (var cc in chemicalCompositions)
            {
                foundAlloy.ChemicalCompositions.Add(cc);
            }

            await context.SaveChangesAsync(ctn);

            return foundAlloy.ChemicalCompositions.Select(MapToResponse).ToArray();
        }

        private AlloyChemicalCompositions MapToEntity(ChemicalCompositionsRequest chemicalComposition, Guid alloyGradeId)
        {
            return new AlloyChemicalCompositions
            {
                Id = Guid.NewGuid(),
                MinValue = chemicalComposition.MinValue,
                MaxValue = chemicalComposition.MaxValue,
                ExactValue = chemicalComposition.ExactValue,
                ChemicalElementId = chemicalComposition.ChemicalElementId,
                AlloyGradeId = alloyGradeId
            };
        }
        private ChemicalCompositionsDto MapToResponse(AlloyChemicalCompositions element)
        {
            return new ChemicalCompositionsDto(
                element.Id,
                element.MinValue,
                element.MaxValue,
                element.ExactValue,
                element.ChemicalElementId);
        }
    }
}
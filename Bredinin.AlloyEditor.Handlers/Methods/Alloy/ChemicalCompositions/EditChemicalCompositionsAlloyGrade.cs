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
            var foundAlloy = await context.AlloyGrades
                .Include(x => x.ChemicalCompositions)
                .SingleOrDefaultAsync(x => x.Id == alloyGradeId, ctn);

            if (foundAlloy == null)
                throw new BusinessException("Alloy not found in db");

            var chemicalCompositions = request
                .Select(MapToEntity)
                .ToArray();

            AlloyChemicalCompositionValidator.ValidateTotalRange(chemicalCompositions);

            foundAlloy.ChemicalCompositions.Clear();
            foundAlloy.ChemicalCompositions = chemicalCompositions;

            await context.SaveChangesAsync(ctn);

            return foundAlloy.ChemicalCompositions.Select(MapToResponse).ToArray();
        }

        private AlloyChemicalCompositions MapToEntity(ChemicalCompositionsRequest chemicalComposition)
        {
            var foundElement = context.DictChemicalElements
                .SingleOrDefault(x => x.Id == chemicalComposition.ChemicalElementId);

            if (foundElement == null)
                throw new BusinessException("ChemicalElement not found in db");

            return new AlloyChemicalCompositions
            {
                Id = Guid.NewGuid(),
                MinValue = chemicalComposition.MinValue,
                MaxValue = chemicalComposition.MaxValue,
                ExactValue = chemicalComposition.ExactValue,
                ChemicalElementId = chemicalComposition.ChemicalElementId,
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

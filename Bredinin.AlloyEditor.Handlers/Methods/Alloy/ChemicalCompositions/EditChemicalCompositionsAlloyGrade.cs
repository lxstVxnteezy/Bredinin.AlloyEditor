using Bredinin.AlloyEditor.Contracts.Common.ChemicalCompositions;
using Bredinin.AlloyEditor.Core.Http.Exceptions;
using Bredinin.AlloyEditor.DAL.Core;
using Bredinin.AlloyEditor.Domain.Alloys;
using Bredinin.AlloyEditor.Domain.Dictionaries;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.AlloyEditor.Handlers.Methods.Alloy.ChemicalCompositions
{
    public interface IEditChemicalCompositionsAlloyGradeHandler : IHandler
    {
        Task<ChemicalCompositionsDto[]> Handle(Guid alloyGradeId,
            ChemicalCompositionsRequest[] request,
            CancellationToken ctn);
    }
    public class EditChemicalCompositionsAlloyGradeHandler(
        IRepository<Domain.Alloys.AlloyGrade> alloyRepository,
        IRepository<DictChemicalElement> chemicalElementRepository) : IEditChemicalCompositionsAlloyGradeHandler
    {
        public async Task<ChemicalCompositionsDto[]> Handle(Guid alloyGradeId, ChemicalCompositionsRequest[] request, CancellationToken ctn)
        {
            var foundAlloy = await alloyRepository.Query
                .Include(x => x.ChemicalCompositions)
                .SingleOrDefaultAsync(x => x.Id == alloyGradeId, ctn);

            if (foundAlloy == null)
                throw new BusinessException("Alloy not found in db");

            var chemicalCompositions = request
                .Select(MapToEntity)
                .ToArray();

            foundAlloy.ChemicalCompositions.Clear();
            foundAlloy.ChemicalCompositions = chemicalCompositions;

            await alloyRepository.SaveChanges(ctn);

            return foundAlloy.ChemicalCompositions.Select(MapToResponse).ToArray();
        }

        private AlloyChemicalCompositions MapToEntity(ChemicalCompositionsRequest chemicalComposition)
        {
            var foundElement = chemicalElementRepository.Query
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

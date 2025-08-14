using Bredinin.AlloyEditor.Contracts.Common.AlloyGrade;
using Bredinin.AlloyEditor.Core.Http.Exceptions;
using Bredinin.AlloyEditor.DAL;
using Bredinin.AlloyEditor.Domain.Alloys;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.AlloyEditor.Handlers.Methods.Alloy.AlloyGrade
{
    public interface ICreateAlloyGradeHandler : IHandler
    {
        Task<Guid> Handle(CreateAlloyGradeRequest request, CancellationToken ctn);
    }
    internal class CreateAlloyGradeHandler(ServiceDbContext context)
        : ICreateAlloyGradeHandler
    {
        public async Task<Guid> Handle(CreateAlloyGradeRequest request, CancellationToken ctn)
        {
            var foundAlloySystem = context.AlloySystems
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == request.AlloySystemId, ctn);

            if (foundAlloySystem == null)
                throw new BusinessException($"alloy system not found {request.AlloySystemId}");

            var foundAlloy = await context.AlloyGrades
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Name == request.Name, ctn);

            if (foundAlloy != null)
                throw new BusinessException("already exist");

            var chemicalCompositions = request.ChemicalCompositions
                .Select(MapToEntity)
                .ToArray();

            var newAlloyGrade = new Domain.Alloys.AlloyGrade()
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                AlloySystemId = request.AlloySystemId,
                ChemicalCompositions = chemicalCompositions
            };

            context.Add(newAlloyGrade);

            await context.SaveChangesAsync(ctn);

            return newAlloyGrade.Id;
        }

        private AlloyChemicalCompositions MapToEntity(CreateChemicalCompositionRequest chemicalComposition)
        {
            var foundElement = context.AlloyChemicalCompositions
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
    }

}

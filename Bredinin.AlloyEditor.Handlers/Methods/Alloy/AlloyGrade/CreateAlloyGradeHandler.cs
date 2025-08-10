using Bredinin.AlloyEditor.Contracts.Common.AlloyGrade;
using Bredinin.AlloyEditor.Core.Http.Exceptions;
using Bredinin.AlloyEditor.DAL.Core;
using Bredinin.AlloyEditor.Domain.Alloys;
using Bredinin.AlloyEditor.Domain.Dictionaries;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.AlloyEditor.Handlers.Methods.Alloy.AlloyGrade
{
    public interface ICreateAlloyGradeHandler : IHandler
    {
        Task<Guid> Handle(CreateAlloyGradeRequest request, CancellationToken ctn);
    }
    internal class CreateAlloyGradeHandler(
        IRepository<Domain.Alloys.AlloyGrade> alloyGradeRepository,
        IRepository<Domain.Alloys.AlloySystem> alloySystemRepository,
        IRepository<DictChemicalElement> chemicalElementRepository)
        : ICreateAlloyGradeHandler
    {
        public async Task<Guid> Handle(CreateAlloyGradeRequest request, CancellationToken ctn)
        {
            var foundAlloySystem = alloySystemRepository.Query
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == request.AlloySystemId, ctn);

            if (foundAlloySystem == null)
                throw new BusinessException($"alloy system not found {request.AlloySystemId}");

            var foundAlloy = await alloyGradeRepository.Query
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

            alloyGradeRepository.Add(newAlloyGrade);

            await alloyGradeRepository.SaveChanges(ctn);

            return newAlloyGrade.Id;
        }

        private AlloyChemicalCompositions MapToEntity(CreateChemicalCompositionRequest chemicalComposition)
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
    }

}

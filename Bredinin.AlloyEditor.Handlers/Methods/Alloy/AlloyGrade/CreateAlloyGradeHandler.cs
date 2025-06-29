using Bredinin.AlloyEditor.Contracts.Common.AlloyGrade;
using Bredinin.AlloyEditor.Core.Http.Exceptions;
using Bredinin.AlloyEditor.DAL.Core;
using Bredinin.AlloyEditor.Domain.Alloys;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.AlloyEditor.Handlers.Methods.Alloy.AlloyGrade
{
    public interface ICreateAlloyGradeHandler : IHandler
    {
        Task<Guid> Handle(CreateAlloyGradeRequest request, CancellationToken ctn);
    }
    internal class CreateAlloyGradeHandler(
        IRepository<Domain.Alloys.AlloyGrade> alloyGradeRepository,
        IRepository<Domain.Alloys.AlloySystem> alloySystemRepository)
        : ICreateAlloyGradeHandler
    {
        public async Task<Guid> Handle(CreateAlloyGradeRequest request, CancellationToken ctn)
        {
            var foundAlloySystem = alloySystemRepository.Query
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == request.AlloySystemId, ctn);

            if (foundAlloySystem == null)
                throw new BusinessException("not found alloySystem");

            var foundAlloy = await alloyGradeRepository.Query
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Name == request.Name, ctn);

            if (foundAlloy != null)
                throw new BusinessException("already exist");

            var newAlloyGrade = new Domain.Alloys.AlloyGrade()
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                AlloySystemId = request.AlloySystemId,
                ChemicalCompositions = request.ChemicalCompositions.Select(element => new AlloyChemicalCompositions()
                {
                    MinValue = element.MinValue,
                    MaxValue = element.MaxValue,
                    ExactValue = element.ExactValue,
                    ChemicalElementId = element.ChemicalElementId,
                }
                ).ToArray()
            };

            alloyGradeRepository.Add(newAlloyGrade);

            await alloyGradeRepository.SaveChanges(ctn);

            return newAlloyGrade.Id;
        }
    }
}

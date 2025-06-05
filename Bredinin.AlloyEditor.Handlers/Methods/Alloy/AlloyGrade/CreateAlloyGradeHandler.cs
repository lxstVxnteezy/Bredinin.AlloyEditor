using Bredinin.AlloyEditor.Contracts.Common.AlloyGrade;
using Bredinin.AlloyEditor.DAL.Core;
using Bredinin.MyPetProject.Domain.Alloys;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.MyPetProject.Handlers.Methods.Alloy.AlloyGrade
{
    public interface ICreateAlloyGradeHandler : IHandler
    {
        Task<Guid> Handle(CreateAlloyGradeRequest request, CancellationToken ctn);
    }
    internal class CreateAlloyGradeHandler(IRepository<Domain.Alloys.AlloyGrade> alloyGradeRepository)
        : ICreateAlloyGradeHandler
    {
        public async Task<Guid> Handle(CreateAlloyGradeRequest request, CancellationToken ctn)
        {
            var data = await alloyGradeRepository.Query.SingleOrDefaultAsync(x => x.Name == request.Name, ctn);

            if (data != null)
                throw new ArgumentException("already exist");

            var chemicalCompositions = request.ChemicalCompositions.Select(cc => new AlloyChemicalCompositions()
            {
                MinValue = cc.MinValue,
                MaxValue = cc.MaxValue,
                ExactValue = cc.ExactValue,
                ChemicalElementId = cc.ChemicalElementId,
            }
            );

            var newAlloyGrade = new Domain.Alloys.AlloyGrade()
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                AlloySystemId = request.AlloySystemId,
                ChemicalCompositions = chemicalCompositions.ToList()
            };

            alloyGradeRepository.Add(newAlloyGrade);

            await alloyGradeRepository.SaveChanges(ctn);

            return newAlloyGrade.Id;
        }
    }
}

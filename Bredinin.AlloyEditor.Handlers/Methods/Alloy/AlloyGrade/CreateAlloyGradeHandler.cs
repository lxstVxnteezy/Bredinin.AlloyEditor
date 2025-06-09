using Bredinin.AlloyEditor.Contracts.Common.AlloyGrade;
using Bredinin.AlloyEditor.Core.Http.Exceptions;
using Bredinin.AlloyEditor.DAL.Core;
using Bredinin.MyPetProject.Domain.Alloys;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.AlloyEditor.Handlers.Methods.Alloy.AlloyGrade
{
    public interface ICreateAlloyGradeHandler : IHandler
    {
        Task<Guid> Handle(CreateAlloyGradeRequest request, CancellationToken ctn);
    }
    internal class CreateAlloyGradeHandler(IRepository<MyPetProject.Domain.Alloys.AlloyGrade> alloyGradeRepository)
        : ICreateAlloyGradeHandler
    {
        public async Task<Guid> Handle(CreateAlloyGradeRequest request, CancellationToken ctn)
        {
            var data = await alloyGradeRepository.Query.SingleOrDefaultAsync(x => x.Name == request.Name, ctn);

            if (data != null)
                throw new BusinessException("already exist");

            var newAlloyGrade = new MyPetProject.Domain.Alloys.AlloyGrade()
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                AlloySystemId = request.AlloySystemId,
                ChemicalCompositions = request.ChemicalCompositions.Select(cc => new AlloyChemicalCompositions()
                {
                    MinValue = cc.MinValue,
                    MaxValue = cc.MaxValue,
                    ExactValue = cc.ExactValue,
                    ChemicalElementId = cc.ChemicalElementId,
                }
                ).ToList()
            };

            alloyGradeRepository.Add(newAlloyGrade);

            await alloyGradeRepository.SaveChanges(ctn);

            return newAlloyGrade.Id;
        }
    }
}

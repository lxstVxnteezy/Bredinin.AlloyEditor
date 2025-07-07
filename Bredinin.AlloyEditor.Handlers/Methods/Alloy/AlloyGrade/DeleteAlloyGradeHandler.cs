using Bredinin.AlloyEditor.Core.Http.Exceptions;
using Bredinin.AlloyEditor.DAL.Core;
using Microsoft.AspNetCore.Mvc;

namespace Bredinin.AlloyEditor.Handlers.Methods.Alloy.AlloyGrade
{
    public interface IDeleteAlloyGradeHandler : IHandler
    {
        public Task<ActionResult> Handle(Guid id, CancellationToken ctn);
    }
    internal class DeleteAlloyGradeHandler(
        IRepository<Domain.Alloys.AlloyGrade> alloyGradeRepository) 
        : IDeleteAlloyGradeHandler
    {
        public async Task<ActionResult> Handle(Guid id, CancellationToken ctn)
        {
            var foundAlloyGrade = await alloyGradeRepository.FoundByIdAsync(id, ctn);

            if (foundAlloyGrade == null)
                throw new BusinessException($"Alloy with data Identifier: {id} not found in database");

            alloyGradeRepository.Remove(foundAlloyGrade);
            
            await alloyGradeRepository.SaveChanges(ctn);

            return new ContentResult();
        }
    }
}

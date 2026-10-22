using Bredinin.AlloyEditor.DAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.AlloyEditor.Handlers.Methods.Alloy.AlloyGrade
{
    public interface IDeleteAlloyGradeHandler : IHandler
    {
        Task<ActionResult> Handle(Guid id, CancellationToken ctn);
    }
    internal class DeleteAlloyGradeHandler(
        ServiceDbContext context)
        : IDeleteAlloyGradeHandler
    {
        public async Task<ActionResult> Handle(Guid id, CancellationToken ctn)
        {
            var deleted = await context.AlloyGrades
                .Where(x => x.Id == id)
                .ExecuteDeleteAsync(ctn);

            return deleted > 0 ? new NoContentResult() : new NotFoundResult();
        }
    }
}
using System.Net;
using Bredinin.AlloyEditor.Common.Http;
using Bredinin.AlloyEditor.DAL;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.AlloyEditor.Handlers.Methods.Alloy.AlloyGrade
{
    public interface IDeleteAlloyGradeHandler : IHandler
    {
        Task Handle(Guid id, CancellationToken ctn);
    }
    internal class DeleteAlloyGradeHandler(
        ServiceDbContext context)
        : IDeleteAlloyGradeHandler
    {
        public async Task Handle(Guid id, CancellationToken ctn)
        {
            var deleted = await context.AlloyGrades
                .Where(x => x.Id == id)
                .ExecuteDeleteAsync(ctn);

            if (deleted == 0)
            {
                throw new BusinessException($"Alloy grade not found: {id}", HttpStatusCode.NotFound);
            }
        }
    }
}

using Bredinin.AlloyEditor.Common.Http;
using Bredinin.AlloyEditor.Contracts.Common.AlloyGrade;
using Bredinin.AlloyEditor.DAL;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.AlloyEditor.Handlers.Methods.Alloy.AlloyGrade;

public interface IUpdateAlloyGradeHandler : IHandler
{
    Task Handle(Guid id, UpdateAlloyGradeRequest request, CancellationToken ctn);
}

internal class UpdateAlloyGradeHandler(ServiceDbContext context) : IUpdateAlloyGradeHandler
{
    public async Task Handle(Guid id, UpdateAlloyGradeRequest request, CancellationToken ctn)
    {
        var alloy = await context.AlloyGrades
            .FirstOrDefaultAsync(x => x.Id == id, ctn);

        if (alloy == null)
            throw new BusinessException($"Alloy grade not found: {id}");

        // Проверка уникальности имени, если оно меняется
        if (!string.IsNullOrWhiteSpace(request.Name) && request.Name != alloy.Name)
        {
            var nameExists = await context.AlloyGrades
                .AnyAsync(x => x.Name == request.Name && x.Id != id, ctn);

            if (nameExists)
                throw new BusinessException($"Alloy grade with name '{request.Name}' already exists");
        }

        alloy.Name = request.Name?.Trim() ?? alloy.Name;
        alloy.Description = request.Description?.Trim() ?? alloy.Description;
        alloy.AlloySystemId = request.AlloySystemId ?? alloy.AlloySystemId;

        await context.SaveChangesAsync(ctn);
    }
}
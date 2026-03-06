using Bredinin.AlloyEditor.DAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.AlloyEditor.Handlers.Methods.Alloy.MechanicalProperties;

public interface IDeleteMechanicalPropertyHandler : IHandler
{
    Task<ActionResult> Handle(Guid id, CancellationToken ctn);
}

internal sealed class DeleteMechanicalPropertyHandler(ServiceDbContext context) : IDeleteMechanicalPropertyHandler
{
    public async Task<ActionResult> Handle(Guid id, CancellationToken ctn)
    {
        var deleted = await context.AlloyMechanicalProperties
            .Where(x => x.Id == id)
            .ExecuteDeleteAsync(ctn);

        return deleted > 0 ? new NoContentResult() : new NotFoundResult();
    }
}
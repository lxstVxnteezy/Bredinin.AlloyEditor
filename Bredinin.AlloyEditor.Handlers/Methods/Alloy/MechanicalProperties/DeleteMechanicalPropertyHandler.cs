using System.Net;
using Bredinin.AlloyEditor.Common.Http;
using Bredinin.AlloyEditor.DAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.AlloyEditor.Handlers.Methods.Alloy.MechanicalProperties;

public interface IDeleteMechanicalPropertyHandler : IHandler
{
    Task Handle(Guid id, CancellationToken ctn);
}

internal sealed class DeleteMechanicalPropertyHandler(ServiceDbContext context) : IDeleteMechanicalPropertyHandler
{
    public async Task Handle(Guid id, CancellationToken ctn)
    {
        var deleted = await context.AlloyMechanicalProperties
            .Where(x => x.Id == id)
            .ExecuteDeleteAsync(ctn);

        if (deleted == 0)
            throw new BusinessException($"Mechanical property not found: {id}", HttpStatusCode.NotFound);
    }
}

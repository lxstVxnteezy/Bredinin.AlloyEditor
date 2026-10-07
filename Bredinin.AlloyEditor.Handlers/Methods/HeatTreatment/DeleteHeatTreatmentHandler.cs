using System.Net;
using Bredinin.AlloyEditor.Common.Http;
using Bredinin.AlloyEditor.DAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.AlloyEditor.Handlers.Methods.HeatTreatment;

public interface IDeleteHeatTreatmentHandler : IHandler
{
    Task Handle(Guid id, CancellationToken ctn);
}

internal sealed class DeleteHeatTreatmentHandler(ServiceDbContext context) : IDeleteHeatTreatmentHandler
{
    public async Task Handle(Guid id, CancellationToken ctn)
    {
        var deleted = await context.DictTypesOfHeatTreatments
            .Where(x => x.Id == id)
            .ExecuteDeleteAsync(ctn);

        if (deleted == 0)
            throw new BusinessException($"Heat treatment not found: {id}", HttpStatusCode.NotFound);

    }
}

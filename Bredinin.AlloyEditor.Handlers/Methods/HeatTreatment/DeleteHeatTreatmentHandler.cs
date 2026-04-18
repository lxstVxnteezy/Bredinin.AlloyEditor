using Bredinin.AlloyEditor.DAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bredinin.AlloyEditor.Handlers.Methods.HeatTreatment;

public interface IDeleteHeatTreatmentHandler : IHandler
{
    Task<ActionResult> Handle(Guid id, CancellationToken ctn);
}

internal sealed class DeleteHeatTreatmentHandler(ServiceDbContext context) : IDeleteHeatTreatmentHandler
{
    public async Task<ActionResult> Handle(Guid id, CancellationToken ctn)
    {
        var treatment = await context.AlloyHeatTreatments
            .Include(x => x.MechanicalProperties)
            .FirstOrDefaultAsync(x => x.Id == id, ctn);

        if (treatment == null)
            return new NotFoundResult();

        // При каскадном удалении свойства удалятся автоматически
        context.AlloyHeatTreatments.Remove(treatment);
        await context.SaveChangesAsync(ctn);

        return new NoContentResult();
    }
}
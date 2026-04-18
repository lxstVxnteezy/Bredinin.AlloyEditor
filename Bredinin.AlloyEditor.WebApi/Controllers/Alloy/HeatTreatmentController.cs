using Bredinin.AlloyEditor.Contracts.Common.HeatTreatment;
using Bredinin.AlloyEditor.Handlers.Methods.Dictionaries;
using Bredinin.AlloyEditor.WebAPI.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace Bredinin.AlloyEditor.WebApi.Controllers.Alloy;

/// <summary>
/// Контроллер для работы со справочником термообработки.
/// </summary>
[Route("api/heat-treatments")]
public class HeatTreatmentController : BaseApiController
{
    /// <summary>
    /// Получить все типы термообработки.
    /// </summary>
    [HttpGet("types")]
    [ProducesResponseType(typeof(HeatTreatmentTypeDto[]), StatusCodes.Status200OK)]
    public Task<HeatTreatmentTypeDto[]> GetHeatTreatmentTypes(
        [FromServices] IGetHeatTreatmentTypesHandler handler,
        CancellationToken ctn)
        => handler.Handle(ctn);
}
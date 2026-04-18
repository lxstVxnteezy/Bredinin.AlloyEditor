using Bredinin.AlloyEditor.Contracts.Common.Dictionaries;
using Bredinin.AlloyEditor.Handlers.Methods.Dictionaries;
using Bredinin.AlloyEditor.WebAPI.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace Bredinin.AlloyEditor.WebApi.Controllers.Alloy;

/// <summary>
/// Контроллер для работы со справочником механических свойств.
/// </summary>
[Route("api/mechanical-properties")]
public class MechanicalPropertyController : BaseApiController
{
    /// <summary>
    /// Получить все типы механических свойств.
    /// </summary>
    [HttpGet("types")]
    [ProducesResponseType(typeof(MechanicalPropertyTypeDto[]), StatusCodes.Status200OK)]
    public Task<MechanicalPropertyTypeDto[]> GetMechanicalPropertyTypes(
        [FromServices] IGetMechanicalPropertyTypesHandler handler,
        CancellationToken ctn)
        => handler.Handle(ctn);
}
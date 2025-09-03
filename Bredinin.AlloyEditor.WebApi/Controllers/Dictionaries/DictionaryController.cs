using Bredinin.AlloyEditor.Contracts.Common.Dictionaries.DictChemicalElements;
using Bredinin.AlloyEditor.Handlers.Methods.Dictionaries.DictChemicalElements;
using Bredinin.AlloyEditor.WebAPI.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace Bredinin.AlloyEditor.WebAPI.Controllers.Dictionaries
{
    /// <summary>
    /// Контроллер для работы со справочниками.
    /// </summary>
    [Route("api/dictionary")]
    public class DictionaryController : BaseApiController
    {
        /// <summary>
        /// Получить справочник химических элементов
        /// </summary>
        /// <remarks>
        /// Возвращает справочник химических элементов.
        /// </remarks>
        /// <returns>Массив объектов <see cref="DictChemicalElementResponse"/>.</returns>
        /// <response code="200">Список химических элементов успешно получен.</response>
        /// <response code="400">Нету зарегистрированных химических элементов.</response>
        [HttpGet("chemical-elements")]
        [ProducesResponseType(typeof(DictChemicalElementResponse[]), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public Task<DictChemicalElementResponse[]?> GetChemicalElements(
            [FromServices] IInfoDictChemicalElementsHandler handler,
            CancellationToken ctn)
        {
            return handler.Handle(ctn);
        }
    }
}


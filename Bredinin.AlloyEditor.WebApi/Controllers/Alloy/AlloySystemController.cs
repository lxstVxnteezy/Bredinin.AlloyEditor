using Bredinin.AlloyEditor.Contracts.Common.AlloyBaseSystem;
using Bredinin.AlloyEditor.Handlers.Methods.Alloy.AlloySystem;
using Bredinin.AlloyEditor.WebAPI.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace Bredinin.AlloyEditor.WebAPI.Controllers.Alloy
{
    /// <summary>
    /// Контроллер для работы со системой сплавов.
    /// </summary>
    [Route("api/alloys-systems")]
    public class AlloySystemController : BaseApiController
    {

        /// <summary>
        /// Получить все системы сплавов
        /// </summary>
        /// <remarks>
        /// Возвращает список всех систем сплавов.
        /// </remarks>
        /// <returns>Массив объектов <see cref="SearchAlloySystemResponse"/>.</returns>
        /// <response code="200">Список систем сплавов успешно получен.</response>
        /// <response code="400">Нету зарегистрированных систем сплавов.</response>
        [HttpGet]
        [ProducesResponseType(typeof(SearchAlloySystemResponse[]), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public Task<SearchAlloySystemResponse[]> GetAlloySystem(
            [FromServices] ISearchAlloySystem handler,
            CancellationToken ctn)
        {
            return handler.Handle(ctn);
        }

    }
}

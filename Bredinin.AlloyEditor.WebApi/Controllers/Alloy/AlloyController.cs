using Bredinin.AlloyEditor.Contracts.Common.AlloyGrade;
using Bredinin.AlloyEditor.Contracts.Common.ChemicalCompositions;
using Bredinin.AlloyEditor.Handlers.Methods.Alloy.AlloyGrade;
using Bredinin.AlloyEditor.Handlers.Methods.Alloy.ChemicalCompositions;
using Bredinin.AlloyEditor.WebAPI.Controllers.Base;
using Microsoft.AspNetCore.Mvc;
#pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)

namespace Bredinin.AlloyEditor.WebAPI.Controllers.Alloy
{
    /// <summary>
    /// Контроллер для работы со сплавами.
    /// </summary>
    [Route("api/alloys")]
    public class AlloyController : BaseApiController
    {
        /// <summary>
        /// Получить все сплавы.
        /// </summary>
        /// <remarks>
        /// Возвращает список всех зарегистрированных сплавов.
        /// </remarks>
        /// <returns>Массив объектов <see cref="AlloyGradeResponse"/>.</returns>
        /// <response code="200">Список сплавов успешно получен.</response>
        /// <response code="400">Нету зарегистрированных сплавов.</response>
        [HttpGet]
        [ProducesResponseType(typeof(AlloyGradeResponse[]), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public Task<AlloyGradeResponse[]> GetGrades(
            [FromServices] ISearchAlloyGradeHandler handler,
            CancellationToken ctn)
        {
            return handler.Handle(ctn);
        }

        /// <summary>
        /// Создать новый сплав.
        /// </summary>
        /// <param name="request">Данные для создания сплава.</param>
        /// <returns>Идентификатор созданного сплава.</returns>
        /// <response code="201">Сплав успешно создан.</response>
        /// <response code="400">Переданы некорректные данные.</response>
        [HttpPost]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public Task<Guid> CreateAlloyGrade(
            [FromServices] ICreateAlloyGradeHandler handler,
            [FromBody] CreateAlloyGradeRequest request,
            CancellationToken ctn)
        {
            return handler.Handle(request, ctn);
        }

        /// <summary>
        /// Удалить сплав по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор сплава.</param>
        /// <response code="204">Сплав удалён.</response>
        /// <response code="404">Сплав не найден.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<ActionResult> Delete(
            [FromServices] IDeleteAlloyGradeHandler handler,
            [FromRoute] Guid id,
            CancellationToken ctn)
        {
            return handler.Handle(id, ctn);
        }

        /// <summary>
        /// Изменить химический состав сплава.
        /// </summary>
        /// <param name="id">Идентификатор сплава.</param>
        /// <param name="request">Массив изменений химического состава.</param>
        /// <returns>Обновлённый массив химического состава.</returns>
        /// <response code="200">Химический состав успешно изменён.</response>
        /// <response code="404">Сплав не найден.</response>
        [HttpPut("chemical-compositions/{id}")]
        [ProducesResponseType(typeof(ChemicalCompositionsDto[]), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<ChemicalCompositionsDto[]> EditChemicalCompositions(
            [FromRoute] Guid id,
            [FromBody] ChemicalCompositionsRequest[] request,
            [FromServices] IEditChemicalCompositionsAlloyGradeHandler handler,
            CancellationToken ctn)
        {
            return handler.Handle(id, request, ctn);
        }

        /// <summary>
        /// Получить список сплавов по идентификатору базового элемента.
        /// </summary>
        /// <param name="id">Идентификатор базового элемента.</param>
        /// <returns>Список сплавов, связанных с базовым элементом.</returns>
        /// <response code="200">Список сплавов успешно получен.</response>
        /// <response code="404">Сплавы не найдены.</response>
        [HttpGet("for-main-element/{id}")]
        [ProducesResponseType(typeof(InfoAlloyGradeByMainResponse[]), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<InfoAlloyGradeByMainResponse[]> GetAlloysForMainElement(
            [FromRoute] Guid id,
            [FromServices] IGetAlloysByMainElementHandler handler,
            CancellationToken ctn)
        {
            return handler.Handle(id, ctn);
        }
    }
}

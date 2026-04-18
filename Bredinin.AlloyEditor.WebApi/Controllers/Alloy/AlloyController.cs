using Bredinin.AlloyEditor.Contracts.Common.AlloyGrade;
using Bredinin.AlloyEditor.Contracts.Common.ChemicalCompositions;
using Bredinin.AlloyEditor.Contracts.Common.HeatTreatment;
using Bredinin.AlloyEditor.Contracts.Common.MechenicalProperties;
using Bredinin.AlloyEditor.Handlers.Methods.Alloy.AlloyGrade;
using Bredinin.AlloyEditor.Handlers.Methods.Alloy.ChemicalCompositions;
using Bredinin.AlloyEditor.Handlers.Methods.Alloy.MechanicalProperties;
using Bredinin.AlloyEditor.Handlers.Methods.HeatTreatment;
using Bredinin.AlloyEditor.WebAPI.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace Bredinin.AlloyEditor.WebAPI.Controllers.Alloy
{
    /// <summary>
    /// Контроллер для работы со сплавами.
    /// </summary>
    [Route("api/alloys")]
    public class AlloyController : BaseApiController
    {
        #region Базовые методы сплавов

        /// <summary>
        /// Получить все сплавы.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(AlloyGradeResponse[]), StatusCodes.Status200OK)]
        public Task<AlloyGradeResponse[]> GetGrades(
            [FromServices] ISearchAlloyGradeHandler handler,
            CancellationToken ctn)
            => handler.Handle(ctn);

        /// <summary>
        /// Получить сплав по идентификатору.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(AlloyGradeResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AlloyGradeResponse>> GetAlloyGradeById(
            [FromServices] IGetAlloyGradeByIdHandler handler,
            Guid id,
            CancellationToken ctn)
        {
            var result = await handler.Handle(id, ctn);
            return Ok(result);
        }

        /// <summary>
        /// Создать новый сплав.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Guid>> CreateAlloyGrade(
            [FromServices] ICreateAlloyGradeHandler handler,
            [FromBody] CreateAlloyGradeRequest request,
            CancellationToken ctn)
        {
            var id = await handler.Handle(request, ctn);
            return CreatedAtAction(nameof(GetAlloyGradeById), new { id }, id);
        }

        /// <summary>
        /// Обновить основную информацию сплава.
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateAlloyGrade(
            [FromServices] IUpdateAlloyGradeHandler handler,
            Guid id,
            [FromBody] UpdateAlloyGradeRequest request,
            CancellationToken ctn)
        {
            await handler.Handle(id, request, ctn);
            return NoContent();
        }

        /// <summary>
        /// Удалить сплав по идентификатору.
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<ActionResult> DeleteAlloyGrade(
            [FromServices] IDeleteAlloyGradeHandler handler,
            Guid id,
            CancellationToken ctn)
            => handler.Handle(id, ctn);

        /// <summary>
        /// Получить список сплавов по идентификатору системы сплавов.
        /// </summary>
        [HttpGet("for-main-element/{id}")]
        [ProducesResponseType(typeof(InfoAlloyGradeByMainResponse[]), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<InfoAlloyGradeByMainResponse[]> GetAlloysForMainElement(
            [FromServices] IGetAlloysByMainElementHandler handler,
            Guid id,
            CancellationToken ctn)
            => handler.Handle(id, ctn);

        #endregion

        #region Химический состав

        /// <summary>
        /// Изменить химический состав сплава.
        /// </summary>
        [HttpPut("{id}/chemical-compositions")]
        [ProducesResponseType(typeof(ChemicalCompositionsDto[]), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<ChemicalCompositionsDto[]> EditChemicalCompositions(
            [FromRoute] Guid id,
            [FromBody] ChemicalCompositionsRequest[] request,
            [FromServices] IEditChemicalCompositionsAlloyGradeHandler handler,
            CancellationToken ctn)
            => handler.Handle(id, request, ctn);

        #endregion

        #region Термообработка

        /// <summary>
        /// Добавить режим термообработки к сплаву.
        /// </summary>
        [HttpPost("{id}/heat-treatments")]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Guid>> AddHeatTreatment(
            [FromServices] IAddHeatTreatmentToAlloyHandler handler,
            Guid id,
            [FromBody] CreateHeatTreatmentRequest request,
            CancellationToken ctn)
        {
            var treatmentId = await handler.Handle(id, request, ctn);
            return CreatedAtAction(nameof(GetAlloyGradeById), new { id }, treatmentId);
        }

        /// <summary>
        /// Обновить режим термообработки.
        /// </summary>
        [HttpPut("heat-treatments/{treatmentId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateHeatTreatment(
            [FromServices] IUpdateHeatTreatmentHandler handler,
            Guid treatmentId,
            [FromBody] UpdateHeatTreatmentRequest request,
            CancellationToken ctn)
        {
            await handler.Handle(treatmentId, request, ctn);
            return NoContent();
        }

        /// <summary>
        /// Удалить режим термообработки.
        /// </summary>
        [HttpDelete("heat-treatments/{treatmentId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<ActionResult> DeleteHeatTreatment(
            [FromServices] IDeleteHeatTreatmentHandler handler,
            Guid treatmentId,
            CancellationToken ctn)
            => handler.Handle(treatmentId, ctn);

        #endregion

        #region Механические свойства

        /// <summary>
        /// Добавить механическое свойство к сплаву.
        /// </summary>
        [HttpPost("{id}/mechanical-properties")]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Guid>> AddMechanicalProperty(
            [FromServices] IAddMechanicalPropertyToAlloyHandler handler,
            Guid id,
            [FromBody] CreateMechanicalPropertyRequest request,
            CancellationToken ctn)
        {
            var propertyId = await handler.Handle(id, request, ctn);
            return CreatedAtAction(nameof(GetAlloyGradeById), new { id }, propertyId);
        }

        /// <summary>
        /// Обновить механическое свойство.
        /// </summary>
        [HttpPut("mechanical-properties/{propertyId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateMechanicalProperty(
            [FromServices] IUpdateMechanicalPropertyHandler handler,
            Guid propertyId,
            [FromBody] UpdateMechanicalPropertyRequest request,
            CancellationToken ctn)
        {
            await handler.Handle(propertyId, request, ctn);
            return NoContent();
        }

        /// <summary>
        /// Удалить механическое свойство.
        /// </summary>
        [HttpDelete("mechanical-properties/{propertyId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<ActionResult> DeleteMechanicalProperty(
            [FromServices] IDeleteMechanicalPropertyHandler handler,
            Guid propertyId,
            CancellationToken ctn)
            => handler.Handle(propertyId, ctn);

        #endregion
    }
}
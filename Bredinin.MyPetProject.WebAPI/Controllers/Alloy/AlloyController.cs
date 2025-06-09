using Bredinin.AlloyEditor.Contracts.Common.AlloyGrade;
using Bredinin.AlloyEditor.WebAPI.Controllers.Base;
using Bredinin.MyPetProject.Handlers.Methods.Alloy.AlloyGrade;
using Microsoft.AspNetCore.Mvc;

namespace Bredinin.AlloyEditor.WebAPI.Controllers.Alloy
{
    [Route("api/alloys")]
    public class AlloyController : BaseApiController
    {
        [HttpGet]
        public Task<AlloyGradeResponse[]> GetGrades(
            [FromServices] ISearchAlloyGradeHandler handler,
            CancellationToken ctn)
        {
            return handler.Handle(ctn);
        }

        [HttpPost]
        public Task<Guid> CreateAlloyGrade(
            [FromServices] ICreateAlloyGradeHandler handler,
            [FromBody] CreateAlloyGradeRequest request,
            CancellationToken ctn)
        {
          return handler.Handle(request, ctn);
        }

        [HttpDelete("{id}")]
        public Task<ActionResult> Delete(
            [FromServices] IDeleteAlloyGradeHandler handler,
            [FromRoute] Guid id,
            CancellationToken ctn)
        {
            return handler.Handle(id,ctn);
        }
    }
}

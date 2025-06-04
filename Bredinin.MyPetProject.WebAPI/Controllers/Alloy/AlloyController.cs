using Bredinin.AlloyEditor.Contracts.Common.AlloyGrade;
using Bredinin.AlloyEditor.WebAPI.Controllers.Base;
using Bredinin.MyPetProject.Handlers.Methods.Alloy.AlloyGrade;
using Microsoft.AspNetCore.Mvc;

namespace Bredinin.AlloyEditor.WebAPI.Controllers.Alloy
{
    [Microsoft.AspNetCore.Components.Route("api/alloys")]
    public class AlloyController : BaseApiController
    {
        [HttpGet("/alloy-info")]
        public Task<AlloyGradeResponse[]> GetGrades(
            [FromServices] ISearchAlloyGradeHandler handler,
            CancellationToken ctn)
        {
            return handler.Handle(ctn);
        }
    }
}

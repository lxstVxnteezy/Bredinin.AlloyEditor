using Bredinin.AlloyEditor.Contracts.Common.AlloyBaseSystem;
using Bredinin.AlloyEditor.Handlers.Methods.Alloy.AlloySystem;
using Bredinin.AlloyEditor.WebAPI.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace Bredinin.AlloyEditor.WebAPI.Controllers.Alloy
{
    [Route("api/alloys-systems")]
    public class AlloySystemController : BaseApiController
    {
        [HttpGet]
        public Task<SearchAlloySystemResponse[]> GetAlloySystem(
            [FromServices] ISearchAlloySystem handler,
            CancellationToken ctn)
        {
            return handler.Handle(ctn);
        }

    }
}

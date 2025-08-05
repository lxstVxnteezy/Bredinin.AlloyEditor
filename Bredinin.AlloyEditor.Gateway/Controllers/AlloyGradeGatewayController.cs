using Bredinin.AlloyEditor.Contracts.Common.AlloyGrade;
using Bredinin.AlloyEditor.Gateway.Core.Clients.ApiClients;
using Microsoft.AspNetCore.Mvc;
using Refit;

namespace Bredinin.AlloyEditor.Gateway.Controllers
{
    [Route("api/gateway/alloy-grade")]
    public class AlloyGradeGatewayController(IAlloyClient alloyClient) : BaseApiController
    {
        [HttpGet]
        public async Task<ActionResult<AlloyGradeResponse[]>> GetGrades(CancellationToken ctn)
        {
            var grades = await alloyClient.GetGrades(ctn);
            return Ok(grades);
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> CreateAlloyGrade(
            [FromBody] CreateAlloyGradeRequest request,
            CancellationToken ctn)
        {
            var id = await alloyClient.CreateAlloyGrade(request, ctn);
            return Ok(id);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAlloyGrade(Guid id, CancellationToken ctn)
        {
            await alloyClient.DeleteAlloyGrade(id, ctn);
            return NoContent();
        }

        [HttpGet("for-main-element/{id}")]
        public async Task<ActionResult<InfoAlloyGradeByMainResponse[]>> GetAlloysForMainElement(
            Guid id,
            CancellationToken ctn)
        {
            var alloys = await alloyClient.GetAlloysForMainElement(id, ctn);
            return Ok(alloys);
        }
    }
}

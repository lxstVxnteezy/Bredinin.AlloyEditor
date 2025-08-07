using Bredinin.AlloyEditor.Contracts.Common.AlloyBaseSystem;
using Bredinin.AlloyEditor.Gateway.Core.Clients.ApiClients;
using Microsoft.AspNetCore.Mvc;

namespace Bredinin.AlloyEditor.Gateway.Controllers
{
    [Route("api/gateway/alloy-system")]

    public class AlloySystemGatewayController(IAlloySystemClient alloySystemClient) : BaseApiController
    {
        [HttpGet]
        public async Task<ActionResult<SearchAlloySystemResponse[]>> GetAlloySystem(CancellationToken ctn)
        {
            var response = await alloySystemClient.GetAlloySystem(ctn);
            return Ok(response);
        }
    }
}

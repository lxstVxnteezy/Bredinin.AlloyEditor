using Bredinin.AlloyEditor.Gateway.Core.Clients.ApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bredinin.AlloyEditor.Gateway.Controllers
{
    [Route("api/gateway/dictionary")]
    [Authorize]
    public class DictionaryGatewayController(IDictionaryClient dictionaryClient) : BaseApiController
    {
        [HttpGet("chemical-elements")]
        public async Task<IActionResult> GetChemicalElements(CancellationToken cancellationToken)
        {
            var elements = await dictionaryClient.GetChemicalElements(cancellationToken);
            return Ok(elements);
        }
    }
}

using Bredinin.AlloyEditor.Gateway.Core.Clients.ApiClients;
using Bredinin.AlloyEditor.Identity.Service.Contracts.Queries.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Bredinin.AlloyEditor.Gateway.Controllers
{
    [Route("api/gateway/identity")]

    public class IdentityGatewayController(IAuthClient client) : BaseApiController
    {
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] GetJwtTokenQuery query, CancellationToken cancellationToken)
        {
            var response = await client.Login(query, cancellationToken);
            return Ok(response);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenQuery query, CancellationToken cancellationToken)
        {
            var response = await client.RefreshToken(query, cancellationToken);
            return Ok(response);
        }
    }
}

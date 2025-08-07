using Bredinin.AlloyEditor.Gateway.Core.Clients.ApiClients;
using Bredinin.AlloyEditor.Identity.Service.Contracts.Commands.Admin;
using Bredinin.AlloyEditor.Identity.Service.Contracts.DTO;
using Bredinin.AlloyEditor.Identity.Service.Contracts.Queries.Admin;
using Microsoft.AspNetCore.Mvc;

namespace Bredinin.AlloyEditor.Gateway.Controllers
{
    [Route("api/gateway/admin")]
    public class AdminGatewayController(IAdminClient adminClient) : BaseApiController
    {
        [HttpPost("user")]
        public async Task<ActionResult<Guid>> CreateUser([FromBody] CreateUserCommand command)
        {
            var response = await adminClient.CreateUser(command);
            return Ok(response);
        }

        [HttpPut("user")]
        public async Task<ActionResult<UpdateUserResponse>> UpdateUser([FromBody] EditUserCommand command)
        {
            var response = await adminClient.UpdateUser(command);
            return Ok(response);
        }

        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUser(Guid userId)
        {
            await adminClient.DeleteUser(userId);
            return NoContent();
        }

        [HttpPost("password-reset")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordUserCommand command)
        {
            await adminClient.ResetPassword(command);
            return Ok();
        }

        [HttpGet("search-users")]
        public async Task<ActionResult<SearchUserQuery[]>> GetAllSearchQueries()
        {
            var response = await adminClient.GetAllSearchQueries();
            return Ok(response);
        }

        [HttpGet("test")]
        public async Task<IActionResult> Test()
        {
            var response = await adminClient.Test();
            return Ok(response);
        }
    }
}

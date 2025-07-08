using Bredinin.AlloyEditor.Identity.Service.Contracts.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Bredinin.AlloyEditor.Identity.Service.Controllers
{
    [Route("api/identity")]
    public class IdentityController(IMediator mediator) : BaseApiController
    {
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] GetJwtTokenQuery query)
        {
            var token = await mediator.Send(query);

            return StatusCode(
                StatusCodes.Status201Created,
                new { Token = token });
        }
    }
}

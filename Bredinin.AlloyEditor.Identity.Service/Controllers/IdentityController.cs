using Bredinin.AlloyEditor.Identity.Service.Contracts.Queries.Auth;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Bredinin.AlloyEditor.Identity.Service.Controllers
{
    [Route("api/identity")]
    public class IdentityController(IMediator mediator) : BaseApiController(mediator)
    {
        [HttpPost("login")]
        public async Task<string> Login([FromBody] GetJwtTokenQuery query)
        {
            var response = await _mediator.Send(query);

            return response;
        }
    }
}

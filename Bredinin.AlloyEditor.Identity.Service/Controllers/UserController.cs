using Bredinin.AlloyEditor.Identity.Service.Contracts.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Bredinin.AlloyEditor.Identity.Service.Controllers
{
    [Route("api/users")]
    public class UserController: BaseApiController
    {
        private readonly IMediator _mediator;

        public UserController(IMediator mediator)
        {
            _mediator = mediator;
        }
        
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
        {
            var userId = await _mediator.Send(command);
            
            return StatusCode(
                StatusCodes.Status201Created,
                new { UserId = userId });
        }

    }
}

using Bredinin.AlloyEditor.Identity.Service.Contracts.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
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

        [HttpGet("test")]
        public async Task<IActionResult> Test()
        {
            await Task.Delay(5000);

            var response = new
            {
                Status = "Success",
                Message = "This is a test endpoint with 5-second delay",
                Timestamp = DateTime.UtcNow,
                Data = new
                {
                    ExampleValue = 42,
                    Description = "This is just sample data"
                }
            };

            return Ok(response);
        }
    }
}

using Bredinin.AlloyEditor.Identity.Service.Contracts.Commands.Admin;
using Bredinin.AlloyEditor.Identity.Service.Contracts.DTO;
using Bredinin.AlloyEditor.Identity.Service.Contracts.Queries.Admin;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Bredinin.AlloyEditor.Identity.Service.Controllers
{
    /// <summary>
    /// Контроллер для работы с пользователями
    /// </summary>
    
    [Route("api/admin")]
    public class AdminController(IMediator mediator) : BaseApiController(mediator)
    {
        /// <summary>
        /// Создать нового пользователя
        /// </summary>
        /// <param name="command">Данные пользователя</param>
        /// <returns>Созданный пользователь</returns>
        /// <response code="201">Пользователь успешно создан</response>
        /// <response code="400">Неверные данные</response>
        [HttpPost("user")]
        public async Task<Guid> CreateUser([FromBody] CreateUserCommand command)
        {
            var response = await _mediator.Send(command);

            return response;
        }
        
        [HttpPut("user")]
        public async Task<UpdateUserResponse> UpdateUser([FromBody] EditUserCommand command)
        {
            var response = await _mediator.Send(command);

            return response;
        }

        /// <summary>
        /// Получить пользователя по ID
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <returns>Данные пользователя</returns>
        /// <response code="200">Пользователь найден</response>
        /// <response code="404">Пользователь не найден</response>
        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUser(Guid userId)
        {
            await _mediator.Send(new DeleteUserCommand(userId));
            return NoContent();
        }

        [HttpPost("password-reset")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordUserCommand command)
        {
            await _mediator.Send(command);

            return Ok();
        }
        
        /// <summary>
        /// Получить всех пользователей
        /// </summary>
        /// <returns>Список пользователей</returns>
        /// <response code="200">Успешное выполнение</response>
        [HttpGet("search-users")]
        public async Task<SearchUserQuery[]> GetAllSearchQueries()
        {
            var response = await _mediator.Send(new GetAllSearchUserQueries());

            return response;
        }
        /// <summary>
        /// TEST
        /// </summary>
        /// <response code="200">Успешное выполнение</response>
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

using Bredinin.AlloyEditor.Identity.Service.Contracts.Queries.Auth;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Bredinin.AlloyEditor.Identity.Service.Controllers
{
    [Route("api/identity")]
    public class IdentityController(IMediator mediator) : BaseApiController(mediator)
    {
        /// <summary>
        /// Аутентификация
        /// </summary>
        /// <param name="query"> Данные для входа</param>
        /// <returns>Авторизован <see cref="AuthResponse"/>.</returns>
        /// <response code="201">Авторизован</response>
        /// <response code="401">Не авторизован</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<AuthResponse> Login([FromBody] GetJwtTokenQuery query)
        {
            var response = await _mediator.Send(query);

            return response;
        }

        /// <summary>
        /// Обновить токен
        /// </summary>
        /// <param name="query"> Данные для входа</param>
        /// <returns>Авторизован <see cref="AuthResponse"/>.</returns>
        /// <response code="201">Авторизован</response>
        /// <response code="401">Не авторизован</response>
        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<AuthResponse> Refresh([FromBody] RefreshTokenQuery query)
        {
            var response = await mediator.Send(query);

            return response;
        }
    }
}

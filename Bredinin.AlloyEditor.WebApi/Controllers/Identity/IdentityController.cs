using Bredinin.AlloyEditor.Contracts.Identity;
using Bredinin.AlloyEditor.Handlers.Methods.Identity;
using Bredinin.AlloyEditor.WebAPI.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace Bredinin.AlloyEditor.WebAPI.Controllers.Identity
{
    [Route("api/identity")]

    public class IdentityController : BaseApiController
    {
        [HttpPost("login")]
        public Task<string> Login(
            [FromServices] ILoginUserHandler handler,
            [FromBody] LoginUserRequest request,
            CancellationToken ctn)
        {
            return handler.Handle(request, ctn);
        }

        [HttpPost("register")]
        public Task<ActionResult> Register(
            [FromServices] IRegisterUserHandler handler,
            [FromBody] RegisterUserRequest request,
            CancellationToken ctn)
        {
            return handler.Handle(request, ctn);
        }
    }
}

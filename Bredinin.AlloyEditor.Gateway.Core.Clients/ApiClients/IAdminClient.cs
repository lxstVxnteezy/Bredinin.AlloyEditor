using Bredinin.AlloyEditor.Identity.Service.Contracts.Commands.Admin;
using Bredinin.AlloyEditor.Identity.Service.Contracts.DTO;
using Bredinin.AlloyEditor.Identity.Service.Contracts.Queries.Admin;
using Microsoft.AspNetCore.Mvc;
using Refit;

namespace Bredinin.AlloyEditor.Gateway.Core.Clients.ApiClients;

public interface IAdminClient
{
    [Post("/api/admin/user")]
    Task<Guid> CreateUser([Body] CreateUserCommand command);

    [Put("/api/admin/user")]
    Task<UpdateUserResponse> UpdateUser([Body] EditUserCommand command);

    [Delete("/api/admin/{userId}")]
    Task<IActionResult> DeleteUser([AliasAs("userId")] Guid userId);

    [Post("/api/admin/password-reset")]
    Task<IActionResult> ResetPassword([Body] ResetPasswordUserCommand command);

    [Get("/api/admin/search-users")]
    Task<SearchUserQuery[]> GetAllSearchQueries();

    [Get("/api/admin/test")]
    Task<dynamic> Test();

}
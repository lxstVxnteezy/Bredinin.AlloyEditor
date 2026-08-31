using Bredinin.AlloyEditor.Identity.Service.Contracts.Queries.Auth;
using Microsoft.AspNetCore.Mvc;
using Refit;

namespace Bredinin.AlloyEditor.Gateway.Core.Clients.ApiClients;

public interface IAuthClient
{
    [Post("/api/identity/login")]
    Task<AuthResponse> Login([Body] GetJwtTokenQuery query, CancellationToken ctn = default);

    [Post("/api/identity/refresh-token")]
    Task<AuthResponse> RefreshToken([Body] RefreshTokenQuery query, CancellationToken ctn = default);

    [Post("/api/identity/{userId}/revoke-access")]
    Task RevokeToken(Guid userId, [Body] RevokeTokenQuery query, CancellationToken ctn = default);
}
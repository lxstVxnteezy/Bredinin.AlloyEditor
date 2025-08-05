using Microsoft.AspNetCore.Http;
using Bredinin.AlloyEditor.Gateway.Core.Clients.ApiClients;
using System.IdentityModel.Tokens.Jwt;
using Bredinin.AlloyEditor.Identity.Service.Contracts.Queries.Auth;
using System.Net.Http.Headers;

namespace Bredinin.AlloyEditor.Gateway.Core.Clients.Handlers;

public class JwtRefreshHandler(IHttpContextAccessor httpContextAccessor, IAuthClient client) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var authHeader = httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();

        if (string.IsNullOrEmpty(authHeader))
            return new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized)
            {
                ReasonPhrase = "Authorization header is missing"
            };


        var token = authHeader.StartsWith("Bearer ")
            ? authHeader.Substring("Bearer ".Length)
            : authHeader;

        var tokenHandler = new JwtSecurityTokenHandler();

        if (!tokenHandler.CanReadToken(token))
            return new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized)
            {
                ReasonPhrase = "Invalid JWT token format"
            };
        
        var jwtToken = tokenHandler.ReadJwtToken(token);

        if (IsTokenExpired(jwtToken))
            return new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized)
            {
                ReasonPhrase = "Token has expired"
            };
        

        if (ShouldRefreshToken(jwtToken))
        {
            try
            {
                var authResponse = await client.RefreshToken(new RefreshTokenQuery(token), cancellationToken);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authResponse.AccessToken);
            }
            catch
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized)
                {
                    ReasonPhrase = "Token refresh failed"
                };
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }

    private static bool ShouldRefreshToken(JwtSecurityToken jwtToken)
    {
        var expirationTime = GetTokenExpiration(jwtToken);
        if (!expirationTime.HasValue)
            return false;

        return (expirationTime.Value - DateTime.UtcNow) <= TimeSpan.FromMinutes(20);
    }

    private static bool IsTokenExpired(JwtSecurityToken jwtToken)
    {
        var expirationTime = GetTokenExpiration(jwtToken);
        return expirationTime.HasValue && expirationTime.Value <= DateTime.UtcNow;
    }

    private static DateTime? GetTokenExpiration(JwtSecurityToken jwtToken)
    {
        var expClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "exp")?.Value;
        if (expClaim == null || !long.TryParse(expClaim, out var unixExpirationTime))
            return null;

        return DateTimeOffset.FromUnixTimeSeconds(unixExpirationTime).UtcDateTime;
    }
}
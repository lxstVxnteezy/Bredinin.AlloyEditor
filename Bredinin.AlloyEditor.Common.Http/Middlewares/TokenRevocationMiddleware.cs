using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;

namespace Bredinin.AlloyEditor.Common.Http.Middlewares;

public class TokenRevocationMiddleware(RequestDelegate next,IDistributedCache cache)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var user = context.User;

        if (user.Identity?.IsAuthenticated == true)
        {
            var userId = user.FindFirst("userId")?.Value;
            var iatClaim = user.FindFirst("iat")?.Value;

            if (userId != null && iatClaim != null)
            {
                var revokedAtStr = await cache.GetStringAsync($"revoked:{userId}");
                
                if (revokedAtStr != null)
                {
                    var revokedAt = long.Parse(revokedAtStr);
                    var tokenIssuedAt = long.Parse(iatClaim);

                    if (tokenIssuedAt < revokedAt)
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return; 
                    }
                }
            }
            
        }
        await next(context);
    }
}
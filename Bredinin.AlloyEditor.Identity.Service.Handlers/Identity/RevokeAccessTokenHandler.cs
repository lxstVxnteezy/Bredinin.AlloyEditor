using System.Net;
using Bredinin.AlloyEditor.Common.Http;
using Bredinin.AlloyEditor.Identity.Service.Authentication.Interfaces;
using Bredinin.AlloyEditor.Identity.Service.Contracts.Queries.Auth;
using Bredinin.AlloyEditor.Identity.Service.DAL.Context;
using Bredinin.AlloyEditor.Identity.Service.Handler.Identity.Base;
using Bredinin.AlloyEditor.Services.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace Bredinin.AlloyEditor.Identity.Service.Handler.Identity;

public class RevokeAccessTokenHandler(
    ITokenService tokenService,
    IdentityDbContext context,
    IDistributedCache cache,
    IConnectionMultiplexer redis,
    JwtOptionsAccessor jwtOptionsAccessor)
    : BaseAuthHandler<RevokeTokenQuery, Unit>(tokenService, context, cache, redis, jwtOptionsAccessor)
{
    public override async Task<Unit> Handle(RevokeTokenQuery request, CancellationToken cancellationToken)
    {
        var exists = await context.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken);

        if (!exists)
        {
            throw new BusinessException("User not found", HttpStatusCode.NotFound);
        }

        await RevokeTokenByUserId(request.UserId, cancellationToken);

        return Unit.Value;
    }
}

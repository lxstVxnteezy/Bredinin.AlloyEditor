using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net;
using Bredinin.AlloyEditor.Gateway.Core.Clients.ApiClients;
using Bredinin.AlloyEditor.Identity.Service.Contracts.Queries.Auth;

namespace Bredinin.AlloyEditor.Gateway.Handlers
{
    public class JwtRefreshHandler(IAuthClient authClient) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var authHeader = request.Headers.Authorization;
           
            if (authHeader?.Scheme != "Bearer" || string.IsNullOrEmpty(authHeader.Parameter))
                return await base.SendAsync(request, cancellationToken);

            var accessToken = authHeader.Parameter;

            if (!IsTokenExpired(accessToken))
                return await base.SendAsync(request, cancellationToken);
            

            try
            {

                var refreshResponse = await authClient.RefreshToken(new RefreshTokenQuery(accessToken), cancellationToken);

                if (refreshResponse?.AccessToken == null)
                {
                    return new HttpResponseMessage(HttpStatusCode.Unauthorized);
                }

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", refreshResponse.AccessToken);
            }
            catch
            {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }

            return await base.SendAsync(request, cancellationToken);
        }

        private bool IsTokenExpired(string token)
        {
            try
            {
                var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
                return jwtToken.ValidTo < DateTime.UtcNow;
            }
            catch
            {
                return true; 
            }
        }
    }
}

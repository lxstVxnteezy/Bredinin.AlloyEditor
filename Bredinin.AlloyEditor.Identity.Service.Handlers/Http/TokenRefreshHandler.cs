using System.IdentityModel.Tokens.Jwt;
using System.Net;
using Bredinin.AlloyEditor.Identity.Service.Authentication.Interfaces;
using Bredinin.AlloyEditor.Identity.Service.Contracts.Queries.Auth;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bredinin.AlloyEditor.Identity.Service.Handlers.Http
{
    public class TokenRefreshHandler : DelegatingHandler
    {
        private readonly ITokenService _tokenService;
        private readonly IMediator _mediator;
        private readonly ILogger<TokenRefreshHandler> _logger;
        private readonly Func<Task<string>> _getAccessToken;
        private readonly Func<Task<string>> _getRefreshToken;

        public TokenRefreshHandler(
            ITokenService tokenService,
            IMediator mediator,
            ILogger<TokenRefreshHandler> logger,
            Func<Task<string>> getAccessToken,
            Func<Task<string>> getRefreshToken)
        {
            _tokenService = tokenService;
            _mediator = mediator;
            _logger = logger;
            _getAccessToken = getAccessToken;
            _getRefreshToken = getRefreshToken;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var accessToken = await _getAccessToken();
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                _logger.LogInformation("Access token expired. Attempting to refresh...");

                var refreshToken = await _getRefreshToken();
                var newTokens = await _mediator.Send(new RefreshTokenQuery(accessToken, refreshToken), cancellationToken);

                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", newTokens.AccessToken);
                response = await base.SendAsync(request, cancellationToken);
            }

            return response;
        }
    }
}
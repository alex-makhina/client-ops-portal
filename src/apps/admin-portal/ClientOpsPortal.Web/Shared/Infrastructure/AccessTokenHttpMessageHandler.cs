using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System.Net;
using System.Net.Http.Headers;

namespace ClientOpsPortal.Web.Shared.Infrastructure
{
    public class AccessTokenHttpMessageHandler : DelegatingHandler
    {
        private readonly IAccessTokenProvider _tokenProvider;
        private readonly NavigationManager _navigationManager;

        public AccessTokenHttpMessageHandler(IAccessTokenProvider tokenProvider, NavigationManager navigationManager)
        {
            _tokenProvider = tokenProvider;
            _navigationManager = navigationManager;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var result = await _tokenProvider.RequestAccessToken();

            if (result.Status == AccessTokenResultStatus.RequiresRedirect)
            {
                _navigationManager.NavigateToLogin(result.InteractiveRequestUrl!, result.InteractionOptions!);
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }

            if (result.TryGetToken(out var token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}

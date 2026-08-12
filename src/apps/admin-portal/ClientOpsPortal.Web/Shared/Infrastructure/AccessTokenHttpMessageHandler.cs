using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System.Net.Http.Headers;

namespace ClientOpsPortal.Web.Shared.Infrastructure
{
    public class AccessTokenHttpMessageHandler : DelegatingHandler
    {
        private readonly IAccessTokenProvider _tokenProvider;

        public AccessTokenHttpMessageHandler(IAccessTokenProvider tokenProvider)
        {
            _tokenProvider = tokenProvider;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var result = await _tokenProvider.RequestAccessToken();

            if (result.TryGetToken(out var token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}

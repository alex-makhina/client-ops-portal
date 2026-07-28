using System.Net.Http.Headers;

namespace ClientOpsPortal.Web.Shared.Infrastructure
{
    public class TokenHttpMessageHandler : DelegatingHandler
    {
        private readonly ITokenStore _tokenStore;

        public TokenHttpMessageHandler(ITokenStore tokenStore)
        {
            _tokenStore = tokenStore;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await _tokenStore.GetTokenAsync();

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}

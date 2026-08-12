using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace ClientOpsPortal.Services.Auth.Client;

public static class DependencyInjection
{
    public static IHttpClientBuilder AddAuthClient(this IServiceCollection services, string baseUrl)
    {
        services.AddTransient<BearerTokenDelegatingHandler>();

        return services.AddHttpClient<IAuthClient, AuthClient>(client =>
        {
            client.BaseAddress = new Uri(baseUrl);
        })
        .AddHttpMessageHandler<BearerTokenDelegatingHandler>();
    }
}

public class BearerTokenDelegatingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public BearerTokenDelegatingHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var authorization = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
        if (!string.IsNullOrEmpty(authorization))
        {
            request.Headers.TryAddWithoutValidation("Authorization", authorization);
        }

        return base.SendAsync(request, cancellationToken);
    }
}

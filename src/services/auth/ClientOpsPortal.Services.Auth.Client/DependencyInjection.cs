using Microsoft.Extensions.DependencyInjection;

namespace ClientOpsPortal.Services.Auth.Client;

public static class DependencyInjection
{
    public static IHttpClientBuilder AddAuthClient(this IServiceCollection services, string baseUrl)
    {
        return services.AddHttpClient<IAuthClient, AuthClient>(client =>
        {
            client.BaseAddress = new Uri(baseUrl);
        });
    }
}

using ClientOpsPortal.Services.Directory.Client;
using Microsoft.Extensions.DependencyInjection;

namespace ClientOpsPortal.Services.Directory.Client
{
    public static class DependencyInjection
    {
        public static IHttpClientBuilder AddServicesDirectoryClient(this IServiceCollection services, string baseUrl)
        {
            return services.AddHttpClient<IServicesDirectoryClient, ServicesDirectoryClient>(client =>
            {
                client.BaseAddress = new Uri(baseUrl);
            });
        }
    }
}

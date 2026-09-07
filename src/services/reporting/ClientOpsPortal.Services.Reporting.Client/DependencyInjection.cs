using Microsoft.Extensions.DependencyInjection;

namespace ClientOpsPortal.Services.Reporting.Client
{
    public static class DependencyInjection
    {
        public static IHttpClientBuilder AddServicesDirectoryClient(this IServiceCollection services, string baseUrl)
        {
            return services.AddHttpClient<IReportingClient, ReportingClient>(client =>
            {
                client.BaseAddress = new Uri(baseUrl);
            });
        }
    }
}

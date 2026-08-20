using Microsoft.Extensions.DependencyInjection;

namespace ClientOpsPortal.Services.SubscriptionHistory.Client
{
    public static class DependencyInjection
    {
        public static IHttpClientBuilder AddSubscriptionHistoryClient(this IServiceCollection services, string baseUrl)
        {
            return services.AddHttpClient<ISubscriptionhistoryClient, SubscriptionHistoryClient>(client =>
            {
                client.BaseAddress = new Uri(baseUrl);
            });
        }
    }
}
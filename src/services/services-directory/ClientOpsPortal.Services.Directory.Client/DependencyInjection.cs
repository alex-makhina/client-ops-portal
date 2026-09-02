using ClientOpsPortal.Services.Directory.Grpc;
using Grpc.Net.Client;
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

        public static IServiceCollection AddDirectoryGrpcClient(this IServiceCollection services, string baseUrl)
        {
            services.AddSingleton(_ => GrpcChannel.ForAddress(baseUrl));
            services.AddSingleton(sp =>
                new DirectoryCatalog.DirectoryCatalogClient(sp.GetRequiredService<GrpcChannel>()));
            services.AddSingleton<IDirectoryGrpcClient, DirectoryGrpcClient>();
            return services;
        }
    }
}

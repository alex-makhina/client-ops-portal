using ClientOpsPortal.Domain.Interfaces.Repositories;
using ClientOpsPortal.Infrastructure.Data.Context;
using ClientOpsPortal.Infrastructure.Data.Interceptors;
using ClientOpsPortal.Infrastructure.Data.Repositories;
using ClientOpsPortal.Infrastructure.Data.Seed;
using ClientOpsPortal.Infrastructure.Data.Seed.App;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClientOpsPortal.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddEFCore(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<AuditableInterceptor>();
            services.AddDbContext<ClientOpsPortalDbContext>((sp, options) =>
            {
                var interceptor = sp.GetRequiredService<AuditableInterceptor>();
                options.UseNpgsql(configuration.GetConnectionString("AppConnection"))
                    .AddInterceptors(interceptor);
            });

            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IReportsRepository, ReportsRepository>();

            services.AddScoped<AppDbSeeder>();
            services.AddScoped<IDatabaseSeeder, DatabaseSeeder>();

            services.Scan(scan => scan
                .FromAssemblies(typeof(DependencyInjection).Assembly)
                .AddClasses(classes => classes.Where(c => c.Name.EndsWith("Repository") && !c.IsAbstract))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            return services;
        }

        public static IServiceCollection AddMessageBus(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    var host = configuration["RabbitMq:Host"] ?? "rabbitmq";
                    var username = configuration["RabbitMq:Username"] ?? "guest";
                    var password = configuration["RabbitMq:Password"] ?? "guest";

                    cfg.Host(host, h =>
                    {
                        h.Username(username);
                        h.Password(password);
                    });
                });
            });

            return services;
        }
    }
}
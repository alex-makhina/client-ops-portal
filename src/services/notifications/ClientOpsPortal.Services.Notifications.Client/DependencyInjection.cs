using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClientOpsPortal.Services.Notifications.Client;

public static class DependencyInjection
{
    /// <summary>Registers the MassTransit producer bus + INotificationPublisher.</summary>
    public static IServiceCollection AddNotificationPublisher(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var rabbit = configuration.GetSection("RabbitMq").Get<RabbitMqOptions>() ?? new RabbitMqOptions();

        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(new Uri($"amqp://{rabbit.Host}:{rabbit.Port}{rabbit.VirtualHost}"), h =>
                {
                    h.Username(rabbit.Username);
                    h.Password(rabbit.Password);
                });
            });
        });

        services.AddScoped<INotificationPublisher, MassTransitNotificationPublisher>();
        return services;
    }
}
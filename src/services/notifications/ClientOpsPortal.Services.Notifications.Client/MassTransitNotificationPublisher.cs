using ClientOpsPortal.Services.Notifications.Contracts;
using MassTransit;

namespace ClientOpsPortal.Services.Notifications.Client;

public class MassTransitNotificationPublisher : INotificationPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;

    public MassTransitNotificationPublisher(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public Task PublishAsync(NotificationMessage message, CancellationToken ct = default)
        => _publishEndpoint.Publish(message, ct);
}
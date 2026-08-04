using ClientOpsPortal.Services.Notifications.Contracts;

namespace ClientOpsPortal.Services.Notifications.Client;

public interface INotificationPublisher
{
    Task PublishAsync(NotificationMessage message, CancellationToken ct = default);
}
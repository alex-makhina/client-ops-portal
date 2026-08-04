using ClientOpsPortal.Services.Notifications.Contracts;

namespace ClientOpsPortal.Services.Notifications.Notifiers;

public interface INotifier
{
    Task SendAsync(NotificationMessage message, CancellationToken ct = default);
}
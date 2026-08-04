using ClientOpsPortal.Services.Notifications.Contracts;

namespace ClientOpsPortal.Services.Notifications.Notifiers;

public class ConsoleNotifier : INotifier
{
    private readonly ILogger<ConsoleNotifier> _logger;

    public ConsoleNotifier(ILogger<ConsoleNotifier> logger) => _logger = logger;

    public Task SendAsync(NotificationMessage message, CancellationToken ct = default)
    {
        _logger.LogInformation("[Notification:{Type}] to {Email} | Login: {Login} | Link: {Link}",
            message.Type, message.RecipientEmail, message.Login, message.ResetLink);
        return Task.CompletedTask;
    }
}
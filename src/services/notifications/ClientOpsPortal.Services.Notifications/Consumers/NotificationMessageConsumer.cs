using ClientOpsPortal.Services.Notifications.Contracts;
using ClientOpsPortal.Services.Notifications.Notifiers;
using MassTransit;

namespace ClientOpsPortal.Services.Notifications.Consumers;

public class NotificationMessageConsumer : IConsumer<NotificationMessage>
{
    private readonly IEnumerable<INotifier> _notifiers;
    private readonly ILogger<NotificationMessageConsumer> _logger;

    public NotificationMessageConsumer(IEnumerable<INotifier> notifiers, ILogger<NotificationMessageConsumer> logger)
    {
        _notifiers = notifiers;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<NotificationMessage> context)
    {
        var message = context.Message;
        _logger.LogInformation("Consumed notification {Id} of type {Type} for {Email}", message.NotificationId, message.Type, message.RecipientEmail);

        foreach (var notifier in _notifiers)
            await notifier.SendAsync(message, context.CancellationToken);
    }
}
using ClientOpsPortal.Services.Notifications.Client;
using ClientOpsPortal.Services.Notifications.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace ClientOpsPortal.Services.Notifications.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationPublisher _publisher;

    public NotificationsController(INotificationPublisher publisher) => _publisher = publisher;

    [HttpPost("test")]
    public async Task<IActionResult> SendTest([FromBody] NotificationMessage message, CancellationToken ct = default)
    {
        message.NotificationId = Guid.NewGuid();
        message.CreatedAt = DateTimeOffset.UtcNow;
        await _publisher.PublishAsync(message, ct);
        return Accepted(new { message = "Notification queued", id = message.NotificationId });
    }
}
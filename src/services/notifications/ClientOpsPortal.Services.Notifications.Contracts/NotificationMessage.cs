namespace ClientOpsPortal.Services.Notifications.Contracts;

public enum NotificationType
{
    PasswordSetLink = 1,
    PasswordResetLink = 2,
    ServiceConnected = 10,
    ServiceDisconnected = 11,
    TariffChanged = 12,
    Custom = 100
}

public class NotificationMessage
{
    public Guid NotificationId { get; set; } = Guid.NewGuid();
    public NotificationType Type { get; set; }
    public required string RecipientEmail { get; set; }
    public string? Login { get; set; }
    public string? ResetLink { get; set; }
    public string? Subject { get; set; }
    public string? Body { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
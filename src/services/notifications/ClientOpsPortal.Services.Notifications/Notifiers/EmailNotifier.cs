using System.Net;
using System.Net.Mail;
using ClientOpsPortal.Services.Notifications.Contracts;
using Microsoft.Extensions.Options;

namespace ClientOpsPortal.Services.Notifications.Notifiers;

public class EmailNotifier : INotifier
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailNotifier> _logger;

    public EmailNotifier(IOptions<EmailSettings> settings, ILogger<EmailNotifier> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendAsync(NotificationMessage message, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.SmtpHost))
        {
            _logger.LogWarning("SMTP not configured — skipping email for notification {Type} to {Email}", message.Type, message.RecipientEmail);
            return;
        }

        var (subject, body) = BuildContent(message);

        try
        {
            using var mail = new MailMessage
            {
                From = new MailAddress(_settings.FromEmail, _settings.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };
            mail.To.Add(new MailAddress(message.RecipientEmail));

            using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
            {
                Credentials = new NetworkCredential(_settings.SmtpUsername, _settings.SmtpPassword),
                EnableSsl = _settings.UseSsl
            };

            await client.SendMailAsync(mail, ct);
            _logger.LogInformation("Email sent to {Email}: {Subject}", message.RecipientEmail, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", message.RecipientEmail);
        }
    }

    private static (string Subject, string Body) BuildContent(NotificationMessage m)
    {
        return m.Type switch
        {
            NotificationType.PasswordSetLink => (
                m.Subject ?? "Welcome to ClientOpsPortal — Set Your Password",
                m.Body ?? $"""
                Hello,

                Your account has been created. Please set your password using the link below:

                Login: {m.Login}
                Set Password: {m.ResetLink}

                Best regards,
                ClientOpsPortal Team
                """),
            NotificationType.PasswordResetLink => (
                m.Subject ?? "Password Reset — ClientOpsPortal",
                m.Body ?? $"""
                Hello {m.Login},

                Use the link below to reset your password:
                {m.ResetLink}

                Best regards,
                ClientOpsPortal Team
                """),
            _ => (
                m.Subject ?? "Notification",
                m.Body ?? string.Empty)
        };
    }
}
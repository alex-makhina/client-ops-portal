using System.Net;
using System.Net.Mail;
using ClientOpsPortal.Application.Interfaces;
using ClientOpsPortal.Application.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClientOpsPortal.Application.Services
{
    public class EmailNotificationService : INotificationService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<EmailNotificationService> _logger;

        public EmailNotificationService(IOptions<EmailSettings> settings, ILogger<EmailNotificationService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task SendPasswordSetLinkAsync(string email, string login, string resetLink, CancellationToken ct = default)
        {
            var subject = "Welcome to ClientOpsPortal - Set Your Password";
            var body = $"""
                Hello,

                Your account has been created. Please set your password using the link below:

                Login: {login}
                Set Password: {resetLink}

                This link will expire after some time. If you did not request this, please ignore this email.

                Best regards,
                ClientOpsPortal Team
                """;

            await SendEmailAsync(email, subject, body, ct);
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body, CancellationToken ct)
        {
            try
            {
                using var message = new MailMessage
                {
                    From = new MailAddress(_settings.FromEmail, _settings.FromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false
                };

                message.To.Add(new MailAddress(toEmail));

                using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
                {
                    Credentials = new NetworkCredential(_settings.SmtpUsername, _settings.SmtpPassword),
                    EnableSsl = _settings.UseSsl
                };

                await client.SendMailAsync(message, ct);

                _logger.LogInformation("Email sent successfully to {Email} with subject '{Subject}'", toEmail, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email} with subject '{Subject}'", toEmail, subject);
                throw;
            }
        }
    }
}

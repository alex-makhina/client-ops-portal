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

        public async Task SendPasswordResetAsync(string email, string temporaryPassword, CancellationToken ct = default)
        {
            var subject = "Password Reset - ClientOpsPortal";
            var body = $"""
                Hello,

                Your password has been reset. Below is your temporary password:

                Temporary Password: {temporaryPassword}

                Please log in and change your password immediately.

                Best regards,
                ClientOpsPortal Team
                """;

            await SendEmailAsync(email, subject, body, ct);
        }

        public async Task SendWelcomeWithPasswordAsync(string email, string login, string password, CancellationToken ct = default)
        {
            var subject = "Welcome to ClientOpsPortal";
            var body = $"""
                Hello,

                Your account has been created. Below are your login credentials:

                Login: {login}
                Password: {password}

                Please log in and change your password after first login.

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

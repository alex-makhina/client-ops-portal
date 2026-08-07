using ClientOpsPortal.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace ClientOpsPortal.Application.Services
{
    public class ConsoleNotificationService : INotificationService
    {
        private readonly ILogger<ConsoleNotificationService> _logger;

        public ConsoleNotificationService(ILogger<ConsoleNotificationService> logger)
        {
            _logger = logger;
        }

        public Task SendPasswordSetLinkAsync(string email, string login, string resetLink, CancellationToken ct = default)
        {
            _logger.LogInformation("[Notification] Password set link for {Email}:\n  Login:     {Login}\n  ResetLink: {ResetLink}",
                email, login, resetLink);
            return Task.CompletedTask;
        }
    }
}

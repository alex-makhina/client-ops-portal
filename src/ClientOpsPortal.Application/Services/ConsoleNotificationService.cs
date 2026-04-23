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

        public Task SendPasswordResetAsync(string email, string temporaryPassword, CancellationToken ct = default)
        {
            _logger.LogInformation("Password reset for {Email}. Temporary password: {Password}", email, temporaryPassword);
            return Task.CompletedTask;
        }

        public Task SendWelcomeWithPasswordAsync(string email, string login, string password, CancellationToken ct = default)
        {
            _logger.LogInformation("User {Email} created. Login: {Login}, Password: {Password}", email, login, password);
            return Task.CompletedTask;
        }
    }
}

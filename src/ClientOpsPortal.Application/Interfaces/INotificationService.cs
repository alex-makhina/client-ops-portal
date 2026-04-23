namespace ClientOpsPortal.Application.Interfaces
{
    public interface INotificationService
    {
        Task SendPasswordResetAsync(string email, string temporaryPassword, CancellationToken ct = default);
        Task SendWelcomeWithPasswordAsync(string email, string login, string password, CancellationToken ct = default);
    }
}

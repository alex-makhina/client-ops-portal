namespace ClientOpsPortal.Application.Interfaces
{
    public interface INotificationService
    {
        Task SendPasswordSetLinkAsync(string email, string login, string resetLink, CancellationToken ct = default);
    }
}

namespace ClientOpsPortal.Web.Features.Shared.Notification
{
    public class AppNotificationService
    {
        public event Action<AppNotification>? OnNotify;

        public void ShowSuccess(string message, string title = "Успешно")
        {
            OnNotify?.Invoke(new AppNotification
            {
                Type = NotificationType.Success,
                Title = title,
                Message = message
            });
        }

        public void ShowError(string message, string title = "Ошибка")
        {
            OnNotify?.Invoke(new AppNotification
            {
                Type = NotificationType.Error,
                Title = title,
                Message = message
            });
        }

        public void ShowWarning(string message, string title = "Предупреждение")
        {
            OnNotify?.Invoke(new AppNotification
            {
                Type = NotificationType.Warning,
                Title = title,
                Message = message
            });
        }

        public void ShowInfo(string message, string title = "Информация")
        {
            OnNotify?.Invoke(new AppNotification
            {
                Type = NotificationType.Info,
                Title = title,
                Message = message
            });
        }
    }

    public class AppNotification
    {
        public NotificationType Type { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public enum NotificationType
    {
        Success,
        Error,
        Warning,
        Info
    }
}
using System.ComponentModel.DataAnnotations;

namespace ClientOpsPortal.Domain.Enums
{
    public enum SubscriptionActionStatus
    {
        [Display(Name = "В обработке")]
        InProgress = 0,
        [Display(Name = "Выполнен")]
        Completed = 1,
        [Display(Name = "Ошибка")]
        Failed = 2,
        [Display(Name = "Аннулирован")]
        Cancelled = 3,
        [Display(Name = "В ожидании отправки на активацию")]
        Pending = 4,
    }
}

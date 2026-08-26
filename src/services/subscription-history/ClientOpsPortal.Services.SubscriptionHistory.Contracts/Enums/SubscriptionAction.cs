using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ClientOpsPortal.Services.SubscriptionHistory.Contracts.Enums
{
    public enum SubscriptionActionType
    {
        [Display(Name = "Подключение услуги")]
        Open = 1,
        [Display(Name = "Отключение услуги")]
        Close = 2,
        [Display(Name = "Смена тарифного плана")]
        TariffChange = 3,
    }

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

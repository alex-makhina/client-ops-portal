using System.ComponentModel.DataAnnotations;

namespace ClientOpsPortal.Domain.Enums
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
}

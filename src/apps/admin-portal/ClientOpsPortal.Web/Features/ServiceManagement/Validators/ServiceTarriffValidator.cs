using ClientOpsPortal.Web.Features.ServiceManagement.Models.Requests;

namespace ClientOpsPortal.Web.Features.ServiceManagement.Validators
{
    public static class ServiceTariffValidator
    {
        public static string? ValidateService(string name, string description, DateTimeOffset beginDate, DateTimeOffset? endDate)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "Введите название услуги";

            if (name.Length < 3 || name.Length > 100)
                return "Название услуги должно содержать от 3 до 100 символов";

            if (string.IsNullOrWhiteSpace(description))
                return "Введите описание услуги";

            if (description.Length < 10 || description.Length > 500)
                return "Описание услуги должно содержать от 10 до 500 символов";

            if (beginDate == default)
                return "Укажите дату начала услуги";

            if (endDate.HasValue && endDate.Value.ToUniversalTime() <= beginDate.ToUniversalTime())
                return "Дата окончания должна быть позже даты начала";

            return null;
        }

        public static string? ValidateTariffPlan(TariffPlanRequest tariff, int index)
        {
            if (tariff == null)
                return $"Тарифный план #{index + 1} не указан";

            if (string.IsNullOrWhiteSpace(tariff.Name))
                return $"Введите название тарифного плана #{index + 1}";

            if (tariff.Name.Length < 3 || tariff.Name.Length > 100)
                return $"Название тарифного плана #{index + 1} должно содержать от 3 до 100 символов";

            if (string.IsNullOrWhiteSpace(tariff.Description))
                return $"Введите описание тарифного плана #{index + 1}";

            if (tariff.Description.Length < 10 || tariff.Description.Length > 500)
                return $"Описание тарифного плана #{index + 1} должно содержать от 10 до 500 символов";

            if (tariff.Price < 0)
                return $"Цена тарифного плана #{index + 1} не может быть отрицательной";

            if (tariff.Price > 999)
                return $"Цена тарифного плана #{index + 1} должна быть в диапазоне от 0 до 999";

            if (tariff.BeginDate == default)
                return $"Укажите дату начала тарифного плана #{index + 1}";

            if (tariff.EndDate.HasValue && tariff.EndDate.Value.ToUniversalTime() <= tariff.BeginDate.ToUniversalTime())
                return $"Дата окончания тарифного плана #{index + 1} должна быть позже даты начала";

            return null;
        }
    }
}
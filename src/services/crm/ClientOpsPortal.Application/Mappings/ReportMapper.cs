using ClientOpsPortal.Application.DTOs.Reports;
using ClientOpsPortal.Domain.Entities;
using ClientOpsPortal.Domain.Models.Reports;

namespace ClientOpsPortal.Application.Mappings
{
    public static class ReportMapper
    {
        public static ServiceStatusReportDto ToServiceStatusReportDto(this ServiceStatusReadModel src)
        {
            return new ServiceStatusReportDto
            {
                ServiceId = src.ServiceId,
                ServiceName = src.ServiceName,
                ServiceDescription = src.ServiceDescription,
                BeginDate = src.BeginDate,
                EndDate = src.EndDate,
                TotalSubscriptions = src.TotalSubscriptions,
                ActiveSubscriptions = src.ActiveSubscriptions,
                InactiveSubscriptions = src.InactiveSubscriptions,
                AverageTariffPrice = src.AverageTariffPrice,
                IsActive = src.EndDate == null || src.EndDate > DateTimeOffset.UtcNow
            };
        }

        public static ActiveSubscriptionReportDto ToActiveSubscriptionReportDto(this Subscription src)
        {
            return new ActiveSubscriptionReportDto
            {
                SubscriptionId = src.Id,
                AbonentId = src.Contract?.AbonentId ?? Guid.Empty,
                AbonentFullName = $"{src.Contract?.Abonent?.LastName} {src.Contract?.Abonent?.FirstName} {src.Contract?.Abonent?.MiddleName}".Trim(),
                AccountNumber = src.Contract?.Abonent?.AccountNumber ?? string.Empty,
                ContractId = src.ContractId,
                ContractNumber = src.Contract?.ContractNumber ?? string.Empty,
                ServiceName = src.Service?.Name ?? string.Empty,
                TariffPlanName = src.TariffPlan?.Name ?? string.Empty,
                TariffPrice = src.TariffPlan?.Price ?? 0,
                BeginDate = src.BeginDate,
                EndDate = src.EndDate
            };
        }
    }
}

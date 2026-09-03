using ClientOpsPortal.Services.Reporting.Contracts.DTOs;

namespace ClientOpsPortal.Services.Reporting.Client
{
    public interface IReportingClient
    {
        Task<IEnumerable<ServiceStatusReportDto>> GetServicesStatusReportAsync(
            string format = "json", 
            CancellationToken ct = default);

        Task<ReportPaginationDto<ActiveSubscriptionReportDto>?> GetActiveSubscriptionsReportAsync(
            int page = 1,
            int pageSize = 50,
            string format = "json",
            CancellationToken ct = default);

        Task<SubscriptionDynamicsReportDto?> GetSubscriptionsDynamicsReportAsync(
            DynamicsReportFilterDto filter,
            string format = "json",
            CancellationToken ct = default);
    }
}

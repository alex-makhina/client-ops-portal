using ClientOpsPortal.Application.DTOs.Reports;

namespace ClientOpsPortal.Application.Interfaces
{
    public interface IReportsService
    {
        Task<IEnumerable<ServiceStatusReportDto>> GetServicesStatusAsync(CancellationToken ct = default);

        Task<ReportPaginationDto<ActiveSubscriptionReportDto>> GetActiveSubscriptionsAsync(
            int page = 1,
            int pageSize = 50,
            CancellationToken ct = default);

        Task<SubscriptionDynamicsReportDto> GetSubscriptionsDynamicsAsync(
            DynamicsReportFilterDto filter,
            CancellationToken ct = default);

        Task<string> ExportToCsvAsync<T>(IEnumerable<T> data, string reportName, CancellationToken ct = default) where T : class;
    }
}

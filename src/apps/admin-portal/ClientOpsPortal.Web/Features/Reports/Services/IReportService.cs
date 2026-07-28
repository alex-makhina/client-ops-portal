using ClientOpsPortal.Web.Features.Reports.Models;

namespace ClientOpsPortal.Web.Features.Reports.Services
{
    public interface IReportService
    {
        Task<List<ServiceStatusReportItem>> GetServicesStatusAsync(string format = "json");
        Task<ReportPaginationResult<ActiveSubscriptionReportItem>> GetActiveSubscriptionsAsync(int page = 1, int pageSize = 50, string format = "json");
        Task<SubscriptionDynamicsReportItem> GetSubscriptionsDynamicsAsync(ReportFilterModel filter, string format = "json");
        Task<byte[]> ExportReportAsync(string reportType, ReportFilterModel? filter = null);
    }
}
using ClientOpsPortal.Domain.Entities;
using ClientOpsPortal.Domain.Models.Reports;

namespace ClientOpsPortal.Domain.Interfaces.Repositories
{
    public interface IReportsRepository
    {
        Task<IEnumerable<ServiceStatusReadModel>> GetServicesWithStatsAsync(CancellationToken ct = default);

        Task<(IEnumerable<Subscription> Items, int TotalCount)> GetActiveSubscriptionsAsync(
            int page,
            int pageSize,
            CancellationToken ct = default);

        Task<IEnumerable<Subscription>> GetSubscriptionsForDynamicsAsync(
            DateTimeOffset dateFrom,
            DateTimeOffset dateTo,
            Guid? serviceId,
            Guid? tariffPlanId,
            CancellationToken ct = default);
    }
}

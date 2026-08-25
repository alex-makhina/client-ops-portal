using ClientOpsPortal.Services.Reporting.Data;
using ClientOpsPortal.Services.Reporting.Contracts.DTOs;
using System.Globalization;
using System.Text;

namespace ClientOpsPortal.Services.Reporting.Services
{
    public class ReportsService : IReportsService
    {
        private readonly ReportsRepository _repository;

        public ReportsService(ReportsRepository repository) => _repository = repository;

        public async Task<IEnumerable<ServiceStatusReportDto>> GetServicesStatusAsync(CancellationToken ct = default)
        {
            var readModels = await _repository.GetServicesWithStatsAsync(ct);
            return readModels.Select(rm => rm.ToServiceStatusReportDto()).ToList();
        }

        public async Task<ReportPaginationDto<ActiveSubscriptionReportDto>> GetActiveSubscriptionsAsync(
            int page = 1, int pageSize = 50, CancellationToken ct = default)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 200);

            var (items, totalCount) = await _repository.GetActiveSubscriptionsAsync(page, pageSize, ct);            
            var dtos = items.Select(s => s.ToActiveSubscriptionReportDto()).ToList();

            return ReportPaginationDto<ActiveSubscriptionReportDto>.Create(dtos, page, pageSize, totalCount);
        }

        public async Task<SubscriptionDynamicsReportDto> GetSubscriptionsDynamicsAsync(
            DynamicsReportFilterDto filter, CancellationToken ct = default)
        {
            var dateFrom = filter.DateFrom ?? DateTimeOffset.UtcNow.AddMonths(-1);
            var dateTo = filter.DateTo ?? DateTimeOffset.UtcNow;

            if (dateFrom > dateTo)
                throw new ArgumentException("DateFrom cannot be greater than DateTo", nameof(filter));

            var subscriptions = await _repository.GetSubscriptionsForDynamicsAsync(
                dateFrom, dateTo, filter.ServiceId, filter.TariffPlanId, ct);

            var newSubs = subscriptions.Count(s => s.BeginDate >= dateFrom && s.BeginDate <= dateTo);
            var closedSubs = subscriptions.Count(s => s.EndDate.HasValue && s.EndDate >= dateFrom && s.EndDate <= dateTo);

            var byService = subscriptions
                .Where(s => s.BeginDate >= dateFrom && s.BeginDate <= dateTo)
                .GroupBy(s => s.Service?.Name ?? "Unknown")
                .ToDictionary(g => g.Key, g => g.Count());

            var totalRevenue = subscriptions
                .Where(s => s.BeginDate >= dateFrom && s.BeginDate <= dateTo)
                .Sum(s => s.TariffPlan?.Price ?? 0);

            var uniqueAbonents = subscriptions
                .Select(s => s.Contract?.AbonentId)
                .Where(id => id.HasValue)
                .Distinct()
                .Count();

            return new SubscriptionDynamicsReportDto
            {
                PeriodStart = dateFrom,
                PeriodEnd = dateTo,
                NewSubscriptions = newSubs,
                ClosedSubscriptions = closedSubs,
                ModifiedSubscriptions = 0,
                SubscriptionsByService = byService,
                TotalRevenue = totalRevenue,
                UniqueAbonents = uniqueAbonents
            };
        }

        public Task<string> ExportToCsvAsync<T>(IEnumerable<T> data, string reportName, CancellationToken ct = default) where T : class
        {
            var sb = new StringBuilder();
            var properties = typeof(T).GetProperties()
                .Where(p => p.PropertyType.IsPrimitive ||
                            p.PropertyType == typeof(string) ||
                            p.PropertyType == typeof(DateTimeOffset) ||
                            p.PropertyType == typeof(DateTimeOffset?) ||
                            p.PropertyType == typeof(decimal) ||
                            p.PropertyType == typeof(int) ||
                            p.PropertyType == typeof(Guid) ||
                            p.PropertyType == typeof(bool))
                .ToList();

            sb.AppendLine(string.Join(",", properties.Select(p => $"\"{p.Name}\"")));

            foreach (var item in data)
            {
                var values = properties.Select(p =>
                {
                    var value = p.GetValue(item);
                    if (value == null) return string.Empty;

                    var str = value switch
                    {
                        DateTimeOffset dto => dto.ToString("dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture),
                        decimal dec => dec.ToString(CultureInfo.InvariantCulture),
                        _ => value.ToString()
                    };

                    return $"\"{str?.Replace("\"", "\"\"")}\"";
                });
                sb.AppendLine(string.Join(",", values));
            }

            return Task.FromResult(sb.ToString());
        }
    }
}

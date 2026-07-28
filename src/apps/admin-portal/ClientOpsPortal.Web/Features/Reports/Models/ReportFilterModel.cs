
namespace ClientOpsPortal.Web.Features.Reports.Models
{
    public class ReportFilterModel
    {
        public DateTimeOffset? DateFrom { get; set; }
        public DateTimeOffset? DateTo { get; set; }
        public Guid? ServiceId { get; set; }
        public Guid? TariffPlanId { get; set; }
        public string GroupBy { get; set; } = "Day";
        public string Format { get; set; } = "json";
    }

    public class ServiceStatusReportItem
    {
        public Guid ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string ServiceDescription { get; set; } = string.Empty;
        public DateTimeOffset BeginDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public int TotalSubscriptions { get; set; }
        public int ActiveSubscriptions { get; set; }
        public int InactiveSubscriptions { get; set; }
        public decimal AverageTariffPrice { get; set; }
        public bool IsActive { get; set; }
    }

    public class ActiveSubscriptionReportItem
    {
        public Guid SubscriptionId { get; set; }
        public Guid AbonentId { get; set; }
        public string AbonentFullName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public Guid ContractId { get; set; }
        public string ContractNumber { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public string TariffPlanName { get; set; } = string.Empty;
        public decimal TariffPrice { get; set; }
        public DateTimeOffset BeginDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
    }

    public class SubscriptionDynamicsReportItem
    {
        public DateTimeOffset PeriodStart { get; set; }
        public DateTimeOffset PeriodEnd { get; set; }
        public int NewSubscriptions { get; set; }
        public int ClosedSubscriptions { get; set; }
        public int ModifiedSubscriptions { get; set; }
        public Dictionary<string, int> SubscriptionsByService { get; set; } = new();
        public decimal TotalRevenue { get; set; }
        public int UniqueAbonents { get; set; }
    }

    public class ReportPaginationResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPrevious => Page > 1;
        public bool HasNext => Page < TotalPages;
    }
}
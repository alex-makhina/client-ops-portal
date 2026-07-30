using System;
using System.Collections.Generic;
using System.Text;

namespace ClientOpsPortal.Services.Reporting.Contracts.DTOs
{
    public class SubscriptionDynamicsReportDto
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

    public class DynamicsReportFilterDto
    {
        public DateTimeOffset? DateFrom { get; set; }
        public DateTimeOffset? DateTo { get; set; }
        public Guid? ServiceId { get; set; }
        public Guid? TariffPlanId { get; set; }
        public GroupByPeriod GroupBy { get; set; } = GroupByPeriod.Day;
    }
}

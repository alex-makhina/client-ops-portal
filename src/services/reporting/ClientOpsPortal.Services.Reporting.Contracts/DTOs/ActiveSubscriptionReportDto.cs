using System;
using System.Collections.Generic;
using System.Text;

namespace ClientOpsPortal.Services.Reporting.Contracts.DTOs
{
    public class ActiveSubscriptionReportDto
    {
        public Guid SubscriptionId { get; set; }
        public Guid AbonentId { get; set; }
        public required string AbonentFullName { get; set; }
        public required string AccountNumber { get; set; }
        public Guid ContractId { get; set; }
        public required string ContractNumber { get; set; }
        public required string ServiceName { get; set; }
        public required string TariffPlanName { get; set; }
        public decimal TariffPrice { get; set; }
        public DateTimeOffset BeginDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }

        public bool IsActive => EndDate == null || EndDate > DateTimeOffset.UtcNow;
        public int DaysActive => IsActive
            ? (int)(DateTimeOffset.UtcNow - BeginDate).TotalDays
            : (int)(EndDate!.Value - BeginDate).TotalDays;
    }
}

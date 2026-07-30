namespace ClientOpsPortal.Services.Reporting.Contracts.DTOs
{
    public class ServiceStatusReportDto
    {
        public Guid ServiceId { get; set; }
        public required string ServiceName { get; set; }
        public required string ServiceDescription { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset BeginDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public int TotalSubscriptions { get; set; }
        public int ActiveSubscriptions { get; set; }
        public int InactiveSubscriptions { get; set; }
        public decimal? AverageTariffPrice { get; set; }
    }
}

namespace ClientOpsPortal.Domain.Models.Reports
{
    public record ServiceStatusReadModel(
    Guid ServiceId,
    string ServiceName,
    string ServiceDescription,
    DateTimeOffset BeginDate,
    DateTimeOffset? EndDate,
    int TotalSubscriptions,
    int ActiveSubscriptions,
    int InactiveSubscriptions,
    decimal? AverageTariffPrice);
}

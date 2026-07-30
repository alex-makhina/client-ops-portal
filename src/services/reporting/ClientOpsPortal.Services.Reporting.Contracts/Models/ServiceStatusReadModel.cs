using System;
using System.Collections.Generic;
using System.Text;

namespace ClientOpsPortal.Services.Reporting.Contracts.Models
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

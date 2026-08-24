namespace ClientOpsPortal.Services.Reporting.Contracts.Events;

public record TariffPlanCreatedEvent(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    Guid ServiceId,
    DateTimeOffset BeginDate,
    DateTimeOffset? EndDate,
    DateTimeOffset OccurredOn
);

public record TariffPlanUpdatedEvent(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    Guid ServiceId,
    DateTimeOffset BeginDate,
    DateTimeOffset? EndDate,
    DateTimeOffset OccurredOn
);

public record TariffPlanDeletedEvent(
    Guid Id,
    DateTimeOffset OccurredOn
);
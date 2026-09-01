namespace ClientOpsPortal.Services.Directory.Contracts.Events;

public record TariffPlanSnapshot(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    Guid ServiceId,
    DateTimeOffset BeginDate,
    DateTimeOffset? EndDate
);

public record ServiceCreatedEvent(
    Guid Id,
    string Name,
    string Description,
    DateTimeOffset BeginDate,
    DateTimeOffset? EndDate,
    List<TariffPlanSnapshot> TariffPlans,
    DateTimeOffset OccurredOn
);

public record ServiceUpdatedEvent(
    Guid Id,
    string Name,
    string Description,
    DateTimeOffset BeginDate,
    DateTimeOffset? EndDate,
    List<TariffPlanSnapshot> TariffPlans,
    DateTimeOffset OccurredOn
);

public record ServiceDeletedEvent(
    Guid Id,
    DateTimeOffset OccurredOn
);
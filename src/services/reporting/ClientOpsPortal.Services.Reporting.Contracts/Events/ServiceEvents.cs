namespace ClientOpsPortal.Services.Reporting.Contracts.Events;

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
    DateTimeOffset CreatedAt,
    string? CreatedBy,
    DateTimeOffset? UpdatedAt,
    string? UpdatedBy,
    List<TariffPlanSnapshot> TariffPlans,
    DateTimeOffset OccurredOn
);

public record ServiceUpdatedEvent(
    Guid Id,
    string Name,
    string Description,
    DateTimeOffset BeginDate,
    DateTimeOffset? EndDate,
    DateTimeOffset CreatedAt,
    string? CreatedBy,
    DateTimeOffset? UpdatedAt,
    string? UpdatedBy,
    List<TariffPlanSnapshot> TariffPlans,
    DateTimeOffset OccurredOn
);

public record ServiceDeletedEvent(
    Guid Id,
    DateTimeOffset OccurredOn
);
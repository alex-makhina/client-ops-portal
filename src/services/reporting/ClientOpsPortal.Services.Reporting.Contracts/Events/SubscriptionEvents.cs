namespace ClientOpsPortal.Services.Reporting.Contracts.Events;

public record SubscriptionCreatedEvent(
    Guid Id,
    Guid ContractId,
    Guid ServiceId,
    Guid TariffPlanId,
    DateTimeOffset BeginDate,
    DateTimeOffset? EndDate,
    DateTimeOffset CreatedAt,
    string? CreatedBy,
    DateTimeOffset? UpdatedAt,
    string? UpdatedBy,
    DateTimeOffset OccurredOn
);

public record SubscriptionUpdatedEvent(
    Guid Id,
    Guid ContractId,
    Guid ServiceId,
    Guid TariffPlanId,
    DateTimeOffset BeginDate,
    DateTimeOffset? EndDate,
    DateTimeOffset CreatedAt,
    string? CreatedBy,
    DateTimeOffset? UpdatedAt,
    string? UpdatedBy,
    DateTimeOffset OccurredOn
);

public record SubscriptionDeletedEvent(
    Guid Id,
    DateTimeOffset OccurredOn
);
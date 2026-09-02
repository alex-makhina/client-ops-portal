namespace ClientOpsPortal.Contracts.Events;

public record SubscriptionCreatedEvent(
    Guid Id,
    Guid ContractId,
    Guid ServiceId,
    Guid TariffPlanId,
    DateTimeOffset BeginDate,
    DateTimeOffset? EndDate,
    DateTimeOffset OccurredOn
);

public record SubscriptionUpdatedEvent(
    Guid Id,
    Guid ContractId,
    Guid ServiceId,
    Guid TariffPlanId,
    DateTimeOffset BeginDate,
    DateTimeOffset? EndDate,
    DateTimeOffset OccurredOn
);

public record SubscriptionDeletedEvent(
    Guid Id,
    DateTimeOffset OccurredOn
);
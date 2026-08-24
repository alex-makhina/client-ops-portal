namespace ClientOpsPortal.Services.Reporting.Contracts.Events;

public record ContractCreatedEvent(
    Guid Id,
    string ContractNumber,
    Guid AbonentId,
    DateTimeOffset BeginDate,
    DateTimeOffset? EndDate,
    DateTimeOffset CreatedAt,
    string? CreatedBy,
    DateTimeOffset? UpdatedAt,
    string? UpdatedBy,
    DateTimeOffset OccurredOn
);

public record ContractUpdatedEvent(
    Guid Id,
    string ContractNumber,
    Guid AbonentId,
    DateTimeOffset BeginDate,
    DateTimeOffset? EndDate,
    DateTimeOffset CreatedAt,
    string? CreatedBy,
    DateTimeOffset? UpdatedAt,
    string? UpdatedBy,
    DateTimeOffset OccurredOn
);

public record ContractDeletedEvent(
    Guid Id,
    DateTimeOffset OccurredOn
);
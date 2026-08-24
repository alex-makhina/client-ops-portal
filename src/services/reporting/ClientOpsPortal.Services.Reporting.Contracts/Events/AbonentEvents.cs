namespace ClientOpsPortal.Services.Reporting.Contracts.Events;

public record AbonentCreatedEvent(
    Guid Id,
    string IdentificationNumber,
    string FirstName,
    string LastName,
    string? MiddleName,
    string FullName,
    Guid UserId,
    string AccountNumber,
    DateTimeOffset CreatedAt,
    string? CreatedBy,
    DateTimeOffset? UpdatedAt,
    string? UpdatedBy,
    DateTimeOffset OccurredOn
);

public record AbonentUpdatedEvent(
    Guid Id,
    string IdentificationNumber,
    string FirstName,
    string LastName,
    string? MiddleName,
    string FullName,
    Guid UserId,
    string AccountNumber,
    DateTimeOffset CreatedAt,
    string? CreatedBy,
    DateTimeOffset? UpdatedAt,
    string? UpdatedBy,
    DateTimeOffset OccurredOn
);

public record AbonentDeletedEvent(
    Guid Id,
    DateTimeOffset OccurredOn
);
namespace ClientOpsPortal.Contracts.Events;

public record AbonentCreatedEvent(
    Guid Id,
    string IdentificationNumber,
    string FirstName,
    string LastName,
    string? MiddleName,
    string FullName,
    Guid UserId,
    string AccountNumber,
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
    DateTimeOffset OccurredOn
);

public record AbonentDeletedEvent(
    Guid Id,
    DateTimeOffset OccurredOn
);
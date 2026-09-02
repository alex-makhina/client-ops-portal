namespace ClientOpsPortal.Contracts.Events;

public record EmployeeCreatedEvent(
    Guid Id,
    string StaffNumber,
    string FirstName,
    string LastName,
    string? MiddleName,
    Guid UserId,
    string Post,
    string? Department,
    bool IsActive,
    DateTimeOffset OccurredOn
);

public record EmployeeUpdatedEvent(
    Guid Id,
    string StaffNumber,
    string FirstName,
    string LastName,
    string? MiddleName,
    Guid UserId,
    string Post,
    string? Department,
    bool IsActive,
    DateTimeOffset OccurredOn
);

public record EmployeeDeletedEvent(
    Guid Id,
    DateTimeOffset OccurredOn
);
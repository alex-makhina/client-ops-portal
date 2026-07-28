namespace ClientOpsPortal.Domain.Interfaces.Entities
{
    public interface IAuditableEntity : ICreationAuditableEntity
    {
        DateTimeOffset? UpdatedAt { get; set; }
        string? UpdatedBy { get; set; }
    }
}

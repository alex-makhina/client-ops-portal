namespace ClientOpsPortal.Domain.Interfaces.Entities
{
    public interface IAuditableEntity : ICreationAuditableEntity
    {
        DateTime? UpdatedAt { get; set; }
        string? UpdatedBy { get; set; }
    }
}

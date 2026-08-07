namespace ClientOpsPortal.Domain.Interfaces.Entities
{
    public interface ICreationAuditableEntity : IBaseEntity
    {
        DateTimeOffset CreatedAt { get; set; }
        string? CreatedBy { get; set; }
    }
}

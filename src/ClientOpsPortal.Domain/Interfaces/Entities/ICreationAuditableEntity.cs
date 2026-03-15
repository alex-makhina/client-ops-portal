namespace ClientOpsPortal.Domain.Interfaces.Entities
{
    public interface ICreationAuditableEntity : IBaseEntity
    {
        DateTime CreatedAt { get; set; }
        string? CreatedBy { get; set; }
    }
}

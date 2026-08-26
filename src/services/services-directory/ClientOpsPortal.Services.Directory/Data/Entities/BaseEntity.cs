namespace ClientOpsPortal.Services.Directory.Data.Entities
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; }
    }

    public abstract class CreationAuditableEntity : BaseEntity
    {
        public DateTimeOffset CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
    }

    public abstract class AuditableEntity : CreationAuditableEntity
    {
        public DateTimeOffset? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}

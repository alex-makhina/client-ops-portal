using ClientOpsPortal.Domain.Interfaces.Entities;

namespace ClientOpsPortal.Domain.Entities.Common
{
    public abstract class AuditableEntity : CreationAuditableEntity, IAuditableEntity
    {
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}

using ClientOpsPortal.Domain.Interfaces.Entities;

namespace ClientOpsPortal.Domain.Entities.Common
{
    public abstract class CreationAuditableEntity : BaseEntity, ICreationAuditableEntity
    {
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
    }
}

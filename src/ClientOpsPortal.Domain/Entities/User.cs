using ClientOpsPortal.Domain.Entities.Common;
using ClientOpsPortal.Domain.Interfaces.Entities;

namespace ClientOpsPortal.Domain.Entities
{
    public class User : AuditableEntity, IUserEntity
    {
        public string? ExternalId { get; set; }

    }
}


namespace ClientOpsPortal.Application.DTOs.Common
{
    public abstract class AuditableDto : CreationAuditableDto
    {
        public DateTimeOffset? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}

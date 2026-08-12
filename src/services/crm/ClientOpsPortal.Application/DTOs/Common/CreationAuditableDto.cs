
namespace ClientOpsPortal.Application.DTOs.Common
{
    public abstract class CreationAuditableDto : BaseDto
    {
        public DateTimeOffset CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
    }
}

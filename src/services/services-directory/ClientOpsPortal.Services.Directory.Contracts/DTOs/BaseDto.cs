using ClientOpsPortal.Services.Directory.Contracts.Models;
using System.ComponentModel.DataAnnotations;

namespace ClientOpsPortal.Services.Directory.Contracts.DTOs
{
    public abstract class BaseDto
    {
        public Guid Id { get; set; }
    }

    public abstract class CreationAuditableDto : BaseDto
    {
        public DateTimeOffset CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
    }

    public abstract class AuditableDto : CreationAuditableDto
    {
        public DateTimeOffset? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}

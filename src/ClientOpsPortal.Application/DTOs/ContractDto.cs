using ClientOpsPortal.Application.DTOs.Common;
using ClientOpsPortal.Domain.Entities;

namespace ClientOpsPortal.Application.DTOs
{
    public class ContractDataDto : AuditableDto
    {
        public required string ContractNumber { get; set; } 
        public Guid AbonentId { get; set; }
        public DateTimeOffset BeginDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
    }

    public class ContractShortDataDto
    {
        public required string ContractNumber { get; set; }
        public Guid AbonentId { get; set; }
        public DateTimeOffset? BeginDate { get; set; }
    }

    public class UpdateContractDto : AuditableDto
    {
        public DateTimeOffset? EndDate { get; set; }
    }
}

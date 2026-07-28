namespace ClientOpsPortal.Web.Features.ClientCard.Models
{
    public class ContractItem
    {
        public Guid Id { get; set; }
        public string ContractNumber { get; set; } = string.Empty;
        public Guid AbonentId { get; set; }
        public DateTimeOffset? BeginDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public bool IsActive => EndDate == null || EndDate > DateTimeOffset.UtcNow;
    }

    public class CreateContractModel
    {
        public string ContractNumber { get; set; } = string.Empty;
        public Guid AbonentId { get; set; }
        public DateTimeOffset BeginDate { get; set; } = DateTimeOffset.UtcNow;
    }

    public class CloseContractModel
    {
        public DateTimeOffset EndDate { get; set; } = DateTimeOffset.UtcNow;
    }
}

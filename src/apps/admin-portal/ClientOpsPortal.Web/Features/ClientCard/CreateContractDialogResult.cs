namespace ClientOpsPortal.Web.Features.ClientCard
{
    public class CreateContractDialogResult
    {
        public string ContractNumber { get; set; } = string.Empty;
        public DateTimeOffset BeginDate { get; set; } = DateTimeOffset.UtcNow;
    }
}

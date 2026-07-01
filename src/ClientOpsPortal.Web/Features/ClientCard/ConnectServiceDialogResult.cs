namespace ClientOpsPortal.Web.Features.ClientCard
{
    public class ConnectServiceDialogResult
    {
        public Guid ServiceId { get; set; }
        public Guid TariffPlanId { get; set; }
        public DateTimeOffset BeginDate { get; set; } = DateTimeOffset.UtcNow;

        public Guid AddressId { get; set; }
        public string AddressText { get; set; } = string.Empty;

    }
}

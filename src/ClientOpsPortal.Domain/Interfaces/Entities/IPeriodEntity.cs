namespace ClientOpsPortal.Domain.Interfaces.Entities
{
    public interface IPeriodEntity : IBaseEntity
    {
        DateTimeOffset BeginDate { get; set; }
        DateTimeOffset? EndDate { get; set; }
    }
}

namespace ClientOpsPortal.Domain.Interfaces.Entities
{
    public interface IPeriodEntity : IBaseEntity
    {
        DateTime BeginDate { get; set; }
        DateTime? EndDate { get; set; }
    }
}

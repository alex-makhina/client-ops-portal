namespace ClientOpsPortal.Domain.Interfaces.Entities
{
    public interface IUserEntity : IBaseEntity
    {
        string? ExternalId { get; set; }
    }
}

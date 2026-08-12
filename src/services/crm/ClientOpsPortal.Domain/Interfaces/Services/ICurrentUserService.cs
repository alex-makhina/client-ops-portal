namespace ClientOpsPortal.Domain.Interfaces.Services
{
    public interface ICurrentUserService
    {
        string ExternalId { get; }
        Guid? UserId { get; }
    }
}

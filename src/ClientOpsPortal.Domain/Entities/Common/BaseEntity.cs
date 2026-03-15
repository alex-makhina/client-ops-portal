using ClientOpsPortal.Domain.Interfaces.Entities;

public abstract class BaseEntity : IBaseEntity
{
    public Guid Id { get; set; }
}

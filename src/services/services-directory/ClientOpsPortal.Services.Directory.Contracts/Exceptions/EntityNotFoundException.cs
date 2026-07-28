namespace ClientOpsPortal.Services.Directory.Contracts.Exceptions
{
    public class EntityNotFoundException : Exception
    {
        public Type EntityType { get; }
        public Guid EntityId { get; }

        public EntityNotFoundException(Type entityType, Guid entityId)
            : base($"{entityType.Name} with Id '{entityId}' was not found.")
        {
            EntityType = entityType;
            EntityId = entityId;
        }

        public EntityNotFoundException(string entityName, Guid entityId)
            : base($"{entityName} with Id '{entityId}' was not found.")
        {
            EntityType = typeof(object);
            EntityId = entityId;
        }
    }

    public class EntityNotFoundException<T>(Guid id) : EntityNotFoundException(typeof(T), id)
    {
    }
}

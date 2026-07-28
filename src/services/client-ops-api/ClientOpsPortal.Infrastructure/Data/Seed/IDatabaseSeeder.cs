namespace ClientOpsPortal.Infrastructure.Data.Seed
{
    public interface IDatabaseSeeder
    {
        Task SeedAllAsync(CancellationToken cancellationToken = default);
    }
}

using Microsoft.EntityFrameworkCore;
using AddressValidator.Domain.Entities;

namespace AddressValidator.Infrastructure.Persistence;

public sealed class AddressDbContext : DbContext
{
    public AddressDbContext(DbContextOptions<AddressDbContext> options) : base(options)
    {
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }
    public DbSet<AddressObject> AddressObjects => Set<AddressObject>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Расширения PostgreSQL (idempotent — CREATE EXTENSION IF NOT EXISTS).
        // postgis:   тип geometry для координат зданий
        // hstore:    нужен osm2pgsql --hstore (плоские таблицы planet_osm_*)
        // uuid-ossp: uuid_generate_v5 для стабильных id
        // pg_trgm:   операторный класс gin_trgm_ops для GIN-индекса на full_path
        modelBuilder.HasPostgresExtension("postgis");
        modelBuilder.HasPostgresExtension("hstore");
        modelBuilder.HasPostgresExtension("uuid-ossp");
        modelBuilder.HasPostgresExtension("pg_trgm");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AddressDbContext).Assembly);
    }

    public async Task<bool> PingAsync(CancellationToken ct = default)
    {
        try { return await Database.CanConnectAsync(ct); }
        catch { return false; }
    }
}

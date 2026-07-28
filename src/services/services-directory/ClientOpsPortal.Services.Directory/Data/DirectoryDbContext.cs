using ClientOpsPortal.Services.Directory.Contracts.Models;
using Microsoft.EntityFrameworkCore;

namespace ClientOpsPortal.Services.Directory.Data
{
    public class DirectoryDbContext : DbContext
    {
        public DirectoryDbContext(DbContextOptions<DirectoryDbContext> options) : base(options)
        {
        }

        public DbSet<Service> Services => Set<Service>();
        public DbSet<TariffPlan> TariffPlans => Set<TariffPlan>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Service>(entity =>
            {
                entity.ToTable("Services");
                entity.Property(e => e.Name).HasMaxLength(50);

                entity.HasMany(s => s.TariffPlans)
                    .WithOne(t => t.Service)
                    .HasForeignKey(t => t.ServiceId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<TariffPlan>(entity =>
            {
                entity.ToTable("TariffPlans");
                entity.Property(e => e.Name).HasMaxLength(50);

                entity.HasOne(t => t.Service)
                    .WithMany(s => s.TariffPlans)
                    .HasForeignKey(t => t.ServiceId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}

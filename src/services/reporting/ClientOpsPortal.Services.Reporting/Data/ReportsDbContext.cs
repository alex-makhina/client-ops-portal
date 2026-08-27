using ClientOpsPortal.Services.Reporting.Contracts.Models;
using Microsoft.EntityFrameworkCore;

namespace ClientOpsPortal.Services.Reporting.Data
{
    public class ReportsDbContext : DbContext
    {
        public ReportsDbContext(DbContextOptions<ReportsDbContext> options) : base(options)
        {
        }

        public DbSet<Service> Services => Set<Service>();
        public DbSet<TariffPlan> TariffPlans => Set<TariffPlan>();
        public DbSet<Subscription> Subscriptions => Set<Subscription>();
        public DbSet<Contract> Contracts => Set<Contract>();
        public DbSet<Abonent> Abonents => Set<Abonent>();
        public DbSet<Employee> Employees => Set<Employee>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Abonent>(entity =>
            {
                entity.Property(e => e.FirstName).HasMaxLength(50);
                entity.Property(e => e.LastName).HasMaxLength(50);
                entity.Property(e => e.MiddleName).HasMaxLength(50);
                entity.Property(e => e.IdentificationNumber).HasMaxLength(20);
                entity.Property(e => e.AccountNumber).HasMaxLength(20);
            });

            modelBuilder.Entity<Employee>(entity =>
            {
                entity.Property(e => e.FirstName).HasMaxLength(50);
                entity.Property(e => e.LastName).HasMaxLength(50);
                entity.Property(e => e.MiddleName).HasMaxLength(50);
                entity.Property(e => e.StaffNumber).HasMaxLength(10);
                entity.Property(e => e.Post).HasMaxLength(100);
                entity.Property(e => e.Department).HasMaxLength(256);
            });

            modelBuilder.Entity<Contract>(entity =>
            {
                entity.Property(e => e.ContractNumber).HasMaxLength(20);
            });

            modelBuilder.Entity<Service>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(50);
            });

            modelBuilder.Entity<TariffPlan>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(50);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}

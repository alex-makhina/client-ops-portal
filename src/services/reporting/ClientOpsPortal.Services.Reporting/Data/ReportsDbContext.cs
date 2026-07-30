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
        public DbSet<User> Users => Set<User>();
        public DbSet<Employee> Employees => Set<Employee>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.ExternalId).HasMaxLength(50);
                entity.Property(e => e.IdentityProvider).HasMaxLength(50);
                entity.Property(e => e.Email).HasMaxLength(256);
                entity.HasIndex(e => e.ExternalId).IsUnique();
            });

            modelBuilder.Entity<Abonent>(entity =>
            {
                entity.Property(e => e.FirstName).HasMaxLength(50);
                entity.Property(e => e.LastName).HasMaxLength(50);
                entity.Property(e => e.MiddleName).HasMaxLength(50);
                entity.Property(e => e.IdentificationNumber).HasMaxLength(20);
                entity.Property(e => e.AccountNumber).HasMaxLength(20);

                entity.HasOne(a => a.User)
                    .WithOne(u => u.Abonent)
                    .HasForeignKey<Abonent>(a => a.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Employee>(entity =>
            {
                entity.Property(e => e.FirstName).HasMaxLength(50);
                entity.Property(e => e.LastName).HasMaxLength(50);
                entity.Property(e => e.MiddleName).HasMaxLength(50);
                entity.Property(e => e.StaffNumber).HasMaxLength(10);
                entity.Property(e => e.Post).HasMaxLength(100);
                entity.Property(e => e.Department).HasMaxLength(256);

                entity.HasOne(e => e.User)
                    .WithOne(u => u.Employee)
                    .HasForeignKey<Employee>(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Contract>(entity =>
            {
                entity.Property(e => e.ContractNumber).HasMaxLength(20);

                entity.HasOne(c => c.Abonent)
                    .WithMany()
                    .HasForeignKey(c => c.AbonentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Service>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(50);

                entity.HasMany(s => s.TariffPlans)
                    .WithOne(t => t.Service)
                    .HasForeignKey(t => t.ServiceId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<TariffPlan>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(50);

                entity.HasOne(t => t.Service)
                    .WithMany(s => s.TariffPlans)
                    .HasForeignKey(t => t.ServiceId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Subscription>(entity =>
            {
                entity.HasOne(s => s.Contract)
                    .WithMany(c => c.Subscriptions)
                    .HasForeignKey(s => s.ContractId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Ignore(s => s.Service);
                entity.Ignore(s => s.TariffPlan);
            });

            //modelBuilder.Entity<SubscriptionHistory>(entity =>
            //{
            //    entity.HasOne(sh => sh.Subscription)
            //        .WithMany()
            //        .HasForeignKey(sh => sh.SubscriptionId)
            //        .OnDelete(DeleteBehavior.Restrict);

            //    // Cross-service reference — populated by DirectoryCacheService, no FK in DB
            //    entity.Ignore(sh => sh.TariffPlan);
            //});

            //modelBuilder.Entity<SubscriptionHistoryStep>(entity =>
            //{
            //    entity.Property(e => e.Message).HasMaxLength(100);
            //});

            base.OnModelCreating(modelBuilder);
        }
    }
}

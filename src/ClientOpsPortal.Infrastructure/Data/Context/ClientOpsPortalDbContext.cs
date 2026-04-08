using ClientOpsPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClientOpsPortal.Infrastructure.Data.Context
{
    public class ClientOpsPortalDbContext : DbContext
    {
        public ClientOpsPortalDbContext(DbContextOptions<ClientOpsPortalDbContext> options)
            : base(options)
        {            
        }

        public DbSet<Service> Services => Set<Service>();
        public DbSet<TariffPlan> TariffPlans => Set<TariffPlan>();
        public DbSet<Subscription> Subscriptions => Set<Subscription>();
        public DbSet<Contract> Contracts => Set<Contract>();
        public DbSet<SubscriptionHistory> SubscriptionHistories => Set<SubscriptionHistory>();
        public DbSet<SubscriptionHistoryStep> SubscriptionHistorySteps => Set<SubscriptionHistoryStep>();
        public DbSet<Abonent> Abonents => Set<Abonent>();
        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Abonent>(entity =>
            {
                entity.Property(e => e.FirstName).HasMaxLength(50);
                entity.Property(e => e.LastName).HasMaxLength(50);
                entity.Property(e => e.MiddleName).HasMaxLength(50);
                entity.Property(e => e.IdentificationNumber).HasMaxLength(20);
                entity.Property(e => e.AccountNumber).HasMaxLength(20);

                entity.HasOne(a => a.User)
                    .WithOne(u => u.Abonent)
                    .HasForeignKey<Abonent>(a => a.UserId);
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
                    .HasForeignKey<Employee>(e => e.UserId);
            });

            modelBuilder.Entity<Contract>().Property(e => e.ContractNumber).HasMaxLength(20);

            modelBuilder.Entity<Service>().Property(e => e.Name).HasMaxLength(50);

            modelBuilder.Entity<SubscriptionHistoryStep>().Property(e => e.Message).HasMaxLength(100);

            modelBuilder.Entity<TariffPlan>().Property(e => e.Name).HasMaxLength(50);

            base.OnModelCreating(modelBuilder);
        }
    }
}

using ClientOpsPortal.Domain.Interfaces.Entities;
using ClientOpsPortal.Domain.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ClientOpsPortal.Infrastructure.Data.Interceptors
{
    public class AuditableInterceptor : SaveChangesInterceptor
    {
        private readonly ICurrentUserService _currentUserService;

        public AuditableInterceptor(ICurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;
        }

        public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
        {
            UpdateData(eventData.Context);
            return base.SavedChanges(eventData, result);
        }

        public override ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
        {
            UpdateData(eventData.Context);
            return base.SavedChangesAsync(eventData, result, cancellationToken);
        }
        private void UpdateData(DbContext? context)
        {
            if(context == null) return;

            var now = DateTimeOffset.UtcNow;
            var userId = _currentUserService.UserId;

            foreach (var entry in context.ChangeTracker.Entries())
            {
                if (entry is ICreationAuditableEntity creationEntity && entry.State == EntityState.Added)
                {
                    creationEntity.CreatedAt = now;
                    creationEntity.CreatedBy = userId;
                }

                if (entry is IAuditableEntity auditableEntity && 
                    (entry.State == EntityState.Modified || entry.State == EntityState.Added))
                {
                    auditableEntity.UpdatedAt = now;
                    auditableEntity.UpdatedBy = userId;
                }    
            }
        }
    }
}

using ClientOpsPortal.Services.Directory.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Security.Claims;

namespace ClientOpsPortal.Services.Directory.Data.Interceptors
{
    public class AuditableInterceptor : SaveChangesInterceptor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditableInterceptor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData, InterceptionResult<int> result)
        {
            UpdateData(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            UpdateData(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void UpdateData(DbContext? context)
        {
            if (context == null) return;

            var now = DateTimeOffset.UtcNow;
            var userId = _httpContextAccessor.HttpContext?.User?
                .FindFirstValue(ClaimTypes.NameIdentifier);

            foreach (var entry in context.ChangeTracker.Entries())
            {
                if (entry.Entity is CreationAuditableEntity creationEntity && entry.State == EntityState.Added)
                {
                    creationEntity.CreatedAt = now;
                    creationEntity.CreatedBy = userId;
                }

                if (entry.Entity is AuditableEntity auditableEntity &&
                    (entry.State == EntityState.Modified || entry.State == EntityState.Added))
                {
                    auditableEntity.UpdatedAt = now;
                    auditableEntity.UpdatedBy = userId;
                }
            }
        }
    }
}

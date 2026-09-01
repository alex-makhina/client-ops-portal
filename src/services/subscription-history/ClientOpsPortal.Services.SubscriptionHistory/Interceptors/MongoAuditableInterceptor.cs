using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using System.Security.Claims;
using ClientOpsPortal.Services.SubscriptionHistory.Contracts.Models;

namespace ClientOpsPortal.Services.SubscriptionHistory.Data.Interceptors
{
    public class MongoAuditableInterceptor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MongoAuditableInterceptor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private string? GetCurrentUserId()
        {
            return _httpContextAccessor.HttpContext?.User?
                .FindFirstValue(ClaimTypes.NameIdentifier);
        }

        private DateTimeOffset GetCurrentTime()
        {
            return DateTimeOffset.UtcNow;
        }

        public void ApplyAuditRules<T>(T entity) where T : BaseEntity
        {
            if (entity == null) return;

            var now = GetCurrentTime();
            var userId = GetCurrentUserId();

            if (entity is CreationAuditableEntity creationEntity)
            {
                if (entity.Id == Guid.Empty)
                {
                    creationEntity.CreatedAt = now;
                    creationEntity.CreatedBy = userId;
                }
            }

            if (entity is AuditableEntity auditableEntity)
            {
                auditableEntity.UpdatedAt = now;
                auditableEntity.UpdatedBy = userId;
            }
        }

        public void ApplyAuditRulesToCollection<T>(IEnumerable<T> entities) where T : BaseEntity
        {
            if (entities == null) return;

            foreach (var entity in entities)
            {
                ApplyAuditRules(entity);
            }
        }
    }
}
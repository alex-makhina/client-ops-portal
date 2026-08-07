using ClientOpsPortal.Domain.Entities;
using ClientOpsPortal.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace ClientOpsPortal.Infrastructure.Data.Repositories
{
    public class SubscriptionRepository(ClientOpsPortalDbContext context) : GenericRepository<Subscription>(context)
    {
        protected override IQueryable<Subscription> ApplyIncludes(IQueryable<Subscription> query)
        {
            return query
                .Include(x => x.Contract);
        }
    }
}
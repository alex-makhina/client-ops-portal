using ClientOpsPortal.Domain.Entities;
using ClientOpsPortal.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClientOpsPortal.Infrastructure.Data.Repositories
{
    public class SubscriptionRepository(ClientOpsPortalDbContext context) : GenericRepository<Subscription>(context)
    {
        protected override IQueryable<Subscription> ApplyIncludes(IQueryable<Subscription> query)
        {
            return query
                .Include(x => x.Service)
                .Include(x => x.TariffPlan);
        }
    }
}

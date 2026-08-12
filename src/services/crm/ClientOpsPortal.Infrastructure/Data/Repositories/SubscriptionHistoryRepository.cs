using ClientOpsPortal.Domain.Entities;
using ClientOpsPortal.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClientOpsPortal.Infrastructure.Data.Repositories
{
    public class SubscriptionHistoryRepository(ClientOpsPortalDbContext context) : GenericRepository<SubscriptionHistory>(context)
    {
        protected override IQueryable<SubscriptionHistory> ApplyIncludes(IQueryable<SubscriptionHistory> query)
        {
            return query
                .Include(x => x.Steps);
        }
    }
}

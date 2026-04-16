using ClientOpsPortal.Domain.Entities;
using ClientOpsPortal.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClientOpsPortal.Infrastructure.Data.Repositories
{
    public class ServiceRepository(ClientOpsPortalDbContext context) : GenericRepository<Service>(context)
    {
        protected override IQueryable<Service> ApplyIncludes(IQueryable<Service> query)
        {
            return query.Include(x => x.TariffPlans);
        }
    }
}

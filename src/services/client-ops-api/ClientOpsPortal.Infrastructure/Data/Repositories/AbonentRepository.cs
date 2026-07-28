using ClientOpsPortal.Domain.Entities;
using ClientOpsPortal.Domain.Interfaces.Repositories;
using ClientOpsPortal.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClientOpsPortal.Infrastructure.Data.Repositories
{
    public class AbonentRepository(ClientOpsPortalDbContext context) : GenericRepository<Abonent>(context)
    {
        protected override IQueryable<Abonent> ApplyIncludes(IQueryable<Abonent> query)
        {
            return query.Include(x => x.User);
        }
    }
}

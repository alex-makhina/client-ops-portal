using ClientOpsPortal.Domain.Entities;
using ClientOpsPortal.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClientOpsPortal.Infrastructure.Data.Repositories
{
    public class EmployeeRepository(ClientOpsPortalDbContext context) : GenericRepository<Employee>(context)
    {
        protected override IQueryable<Employee> ApplyIncludes(IQueryable<Employee> query)
        {
            return query.Include(x => x.User);
        }
    }
}

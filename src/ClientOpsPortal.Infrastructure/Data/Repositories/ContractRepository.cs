using ClientOpsPortal.Domain.Entities;
using ClientOpsPortal.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace ClientOpsPortal.Infrastructure.Data.Repositories
{
    public class ContractRepository(ClientOpsPortalDbContext context) : GenericRepository<Contract>(context)
    {
        protected override IQueryable<Contract> ApplyIncludes(IQueryable<Contract> query)
        {
            return query
                .Include(x => x.Abonent)
                .Include(x => x.Subscriptions);
        }
    }
}

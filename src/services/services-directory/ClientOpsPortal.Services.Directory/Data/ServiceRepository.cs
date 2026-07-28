using ClientOpsPortal.Services.Directory.Contracts.Models;
using ClientOpsPortal.Services.Directory.Data;
using Microsoft.EntityFrameworkCore;

namespace ClientOpsPortal.Services.Directory.Data
{
    public class ServiceRepository : GenericRepository<Service>
    {
        public ServiceRepository(DirectoryDbContext context) : base(context)
        {
        }

        protected override IQueryable<Service> ApplyIncludes(IQueryable<Service> query)
            => query.Include(x => x.TariffPlans);
    }
}

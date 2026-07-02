using ClientOpsPortal.Domain.Entities;
using ClientOpsPortal.Infrastructure.Data.Context;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClientOpsPortal.Infrastructure.Data.Repositories
{
    public class AddressRepository(ClientOpsPortalDbContext context): GenericRepository<Address>(context)
    {
    }
}

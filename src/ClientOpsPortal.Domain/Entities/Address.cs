using ClientOpsPortal.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClientOpsPortal.Domain.Entities
{
    public class Address: AuditableEntity
    {
        public string AddressText { get; set; } = string.Empty;
    }
}

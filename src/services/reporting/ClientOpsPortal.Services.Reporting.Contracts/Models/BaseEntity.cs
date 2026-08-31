using System;
using System.Collections.Generic;
using System.Text;

namespace ClientOpsPortal.Services.Reporting.Contracts.Models
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; }
    }
}

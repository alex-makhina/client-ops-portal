using System;
using System.Collections.Generic;
using System.Text;

namespace ClientOpsPortal.Services.Reporting.Contracts.Models
{
    public class User : BaseEntity
    {
        public string ExternalId { get; set; } = string.Empty;
        public string IdentityProvider { get; set; } = "Identity";
        public string Email { get; set; } = string.Empty;

        public Abonent? Abonent { get; set; }
        public Employee? Employee { get; set; }
    }
}

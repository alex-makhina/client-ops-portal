using Microsoft.AspNetCore.Identity;

namespace ClientOpsPortal.Domain.Entities
{
    public class ApplicationRole : IdentityRole<Guid>
    {
        public string? Description { get; set; }
    }
}

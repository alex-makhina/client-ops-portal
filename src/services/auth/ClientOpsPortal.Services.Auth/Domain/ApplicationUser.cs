using Microsoft.AspNetCore.Identity;
namespace ClientOpsPortal.Services.Auth.Domain;
public class ApplicationUser : IdentityUser<Guid> { }
public class ApplicationRole : IdentityRole<Guid>
{
    public string? Description { get; set; }
}
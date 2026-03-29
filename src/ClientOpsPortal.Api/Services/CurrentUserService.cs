using ClientOpsPortal.Domain.Interfaces.Services;

namespace ClientOpsPortal.Api.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        public string? UserId => "1";
    }
}

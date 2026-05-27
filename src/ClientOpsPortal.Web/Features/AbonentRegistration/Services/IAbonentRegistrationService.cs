using ClientOpsPortal.Web.Features.AbonentRegistration.Models;

namespace ClientOpsPortal.Web.Features.AbonentRegistration.Services
{
    public interface IAbonentRegistrationService
    {
        Task<Guid?> RegisterAbonentAsync(CreateAbonentRequest request);
    }
}

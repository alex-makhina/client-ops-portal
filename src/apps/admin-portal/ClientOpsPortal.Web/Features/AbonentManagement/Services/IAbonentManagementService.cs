using ClientOpsPortal.Web.Features.AbonentManagement.Models;
using ClientOpsPortal.Web.Features.SharedDialog.EditAbonentDialog.Models;

namespace ClientOpsPortal.Web.Features.AbonentManagement.Services
{
    public interface IAbonentManagementService
    {
        Task<IReadOnlyCollection<AbonentShortResult>> SearchByNameAsync(string searchTerm);
        Task<Guid?> RegisterAbonentAsync(AbonentRequest request);
    }
}

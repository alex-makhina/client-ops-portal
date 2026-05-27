using ClientOpsPortal.Web.Features.AbonentSearch.Models;

namespace ClientOpsPortal.Web.Features.AbonentSearch.Services
{
    public interface IAbonentSearchService
    {
        Task<IReadOnlyCollection<AbonentSearchResult>> SearchByNameAsync(string searchTerm);
    }
}

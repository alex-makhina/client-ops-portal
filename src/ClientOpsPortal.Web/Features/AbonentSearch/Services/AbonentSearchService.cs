using ClientOpsPortal.Web.Features.AbonentSearch.Models;
using System.Net.Http.Json;

namespace ClientOpsPortal.Web.Features.AbonentSearch.Services
{
    public class AbonentSearchService : IAbonentSearchService
    {
        private readonly HttpClient _httpClient;

        public AbonentSearchService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("Api");
        }

        public async Task<IReadOnlyCollection<AbonentSearchResult>> SearchByNameAsync(string searchTerm)
        {
            var response = await _httpClient.GetAsync($"api/v1/Abonents/search/by-name?searchTerm={Uri.EscapeDataString(searchTerm)}");

            if (!response.IsSuccessStatusCode)
                return Array.Empty<AbonentSearchResult>();

            var results = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<AbonentSearchResult>>();
            return results ?? Array.Empty<AbonentSearchResult>();
        }
    }
}

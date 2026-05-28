using ClientOpsPortal.Web.Features.AbonentManagement.Models;
using ClientOpsPortal.Web.Features.SharedDialog.EditAbonentDialog.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace ClientOpsPortal.Web.Features.AbonentManagement.Services
{
    public class AbonentManagementService : IAbonentManagementService
    {
        private readonly HttpClient _httpClient;

        public AbonentManagementService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("Api");
        }

        public async Task<IReadOnlyCollection<AbonentShortResult>> SearchByNameAsync(string searchTerm)
        {
            var response = await _httpClient.GetAsync($"api/v1/Abonents/search/by-name?searchTerm={Uri.EscapeDataString(searchTerm)}");

            if (!response.IsSuccessStatusCode)
                return Array.Empty<AbonentShortResult>();

            var results = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<AbonentShortResult>>();
            return results ?? Array.Empty<AbonentShortResult>();
        }

        public async Task<Guid?> RegisterAbonentAsync(AbonentRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/Abonents/register", request);
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("id", out var idProp))
            {
                return idProp.GetGuid();
            }

            return null;
        }
    }
}

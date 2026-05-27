using ClientOpsPortal.Web.Features.AbonentRegistration.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace ClientOpsPortal.Web.Features.AbonentRegistration.Services
{
    public class AbonentRegistrationService : IAbonentRegistrationService
    {
        private readonly HttpClient _httpClient;

        public AbonentRegistrationService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("Api");
        }

        public async Task<Guid?> RegisterAbonentAsync(CreateAbonentRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/Abonents/register", request);
            if (!response.IsSuccessStatusCode)
                return null;

            // The API returns 201 Created with the abonent DTO in the body
            // We need to extract the Id from the response
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

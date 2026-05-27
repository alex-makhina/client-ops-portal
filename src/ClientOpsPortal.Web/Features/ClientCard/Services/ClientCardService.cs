using ClientOpsPortal.Web.Features.ClientCard.Models;
using System.Net.Http.Json;

namespace ClientOpsPortal.Web.Features.ClientCard.Services
{
    public class ClientCardService : IClientCardService
    {
        private readonly HttpClient _httpClient;

        public ClientCardService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("Api");
        }

        public async Task<AbonentDetail?> GetAbonentAsync(Guid abonentId)
        {
            var response = await _httpClient.GetAsync($"api/v1/Abonents/{abonentId}?withIncludes=true");
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<AbonentDetail>();
        }

        public async Task<bool> UpdateAbonentAsync(Guid abonentId, UpdateAbonentModel model)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/v1/Abonents/{abonentId}", model);
            return response.IsSuccessStatusCode;
        }

        public async Task<IReadOnlyCollection<ContractItem>> GetContractsAsync(Guid abonentId)
        {
            var response = await _httpClient.GetAsync($"api/v1/Contracts/by-abonent/{abonentId}?withIncludes=true");
            if (!response.IsSuccessStatusCode)
                return Array.Empty<ContractItem>();

            var contracts = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<ContractItem>>();
            return contracts ?? Array.Empty<ContractItem>();
        }

        public async Task<bool> CreateContractAsync(CreateContractModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/Contracts", model);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ConnectServiceAsync(ConnectServiceModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/Subscriptions/connect", model);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.Error.WriteLine($"ConnectService error ({response.StatusCode}): {error}");
            }
            return response.IsSuccessStatusCode;
        }

        public async Task<IReadOnlyCollection<SubscriptionItem>> GetSubscriptionsByContractAsync(Guid contractId)
        {
            var response = await _httpClient.GetAsync($"api/v1/Subscriptions/by-contract/{contractId}");
            if (!response.IsSuccessStatusCode)
                return Array.Empty<SubscriptionItem>();

            var subscriptions = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<SubscriptionItem>>();
            return subscriptions ?? Array.Empty<SubscriptionItem>();
        }

        public async Task<bool> CancelSubscriptionAsync(Guid subscriptionId)
        {
            var response = await _httpClient.PatchAsync($"api/v1/Subscriptions/{subscriptionId}/cancel", null);
            return response.IsSuccessStatusCode;
        }
    }
}

using ClientOpsPortal.Web.Features.ClientCard.Models;
using ClientOpsPortal.Web.Features.SharedDialog.EditAbonentDialog.Models;
using System.Net.Http.Json;
using System.Text.Json;

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

        public async Task<bool> UpdateAbonentAsync(Guid abonentId, AbonentRequest model)
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

        public async Task<(bool success, string errorMessage)> CloseContractAsync(Guid contractId, CloseContractModel request)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/v1/Contracts/{contractId}", request);

            if (response.IsSuccessStatusCode)
                return (true, string.Empty);

            var errorContent = await response.Content.ReadAsStringAsync();

            if (!string.IsNullOrEmpty(errorContent) && !errorContent.TrimStart().StartsWith("{") && !errorContent.TrimStart().StartsWith("["))
                return (false, errorContent);

            return (false, $"Ошибка при закрытии договора (статус: {(int)response.StatusCode})");
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

        public async Task<IReadOnlyCollection<SubscriptionHistoryItem>> GetSubscriptionHistoryAsync(Guid subscriptionId)
        {
            try
            {
                var url = $"api/v1/SubscriptionHistories/by-subscription/{subscriptionId}";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Array.Empty<SubscriptionHistoryItem>();
                }

                var content = await response.Content.ReadAsStringAsync();

                var rawData = JsonSerializer.Deserialize<List<SubscriptionHistoryDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (rawData == null || rawData.Count == 0)
                    return Array.Empty<SubscriptionHistoryItem>();

                var result = rawData.Select(x => new SubscriptionHistoryItem
                {
                    Id = x.Id,
                    SubscriptionId = x.SubscriptionId,
                    ActionType = ((SubscriptionActionType)x.ActionType).ToString(),
                    Status = ((SubscriptionActionStatus)x.Status).ToString(),
                    StartDate = x.StartDate,
                    CreatedAt = x.CreatedAt,
                    ServiceName = x.ServiceName,
                    TariffPlanName = x.TariffPlanName,
                    Steps = x.Steps?.Select(s => new SubscriptionHistoryStepItem
                    {
                        Id = s.Id,
                        SubscriptionHistoryId = s.SubscriptionHistoryId,
                        Status = ((SubscriptionActionStatus)s.Status).ToString(),
                        Message = s.Message,
                        CreatedAt = s.CreatedAt
                    }).ToList() ?? new()
                }).ToList();

                return result;
            }
            catch (Exception)
            {
                return Array.Empty<SubscriptionHistoryItem>();
            }
        }

        public async Task<IReadOnlyCollection<SubscriptionHistoryItem>> GetSubscriptionHistoryByAbonentAsync(Guid abonentId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/v1/SubscriptionHistories/by-abonent/{abonentId}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return Array.Empty<SubscriptionHistoryItem>();
                }

                var content = await response.Content.ReadAsStringAsync();
                var rawData = JsonSerializer.Deserialize<List<SubscriptionHistoryDto>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (rawData == null || rawData.Count == 0)
                    return Array.Empty<SubscriptionHistoryItem>();

                var result = rawData.Select(x => new SubscriptionHistoryItem
                {
                    Id = x.Id,
                    SubscriptionId = x.SubscriptionId,
                    ActionType = ((SubscriptionActionType)x.ActionType).ToString(),
                    Status = ((SubscriptionActionStatus)x.Status).ToString(),
                    StartDate = x.StartDate,
                    CreatedAt = x.CreatedAt,
                    ServiceName = x.ServiceName,
                    TariffPlanName = x.TariffPlanName,
                    ContractNum = x.ContractNumber,
                    Steps = x.Steps?.Select(s => new SubscriptionHistoryStepItem
                    {
                        Id = s.Id,
                        SubscriptionHistoryId = s.SubscriptionHistoryId,
                        Status = ((SubscriptionActionStatus)s.Status).ToString(),
                        Message = s.Message,
                        CreatedAt = s.CreatedAt
                    }).ToList() ?? new()
                }).ToList();

                return result;
            }
            catch (Exception)
            {
                return Array.Empty<SubscriptionHistoryItem>();
            }
        }

        public async Task<bool> ChangeTariffPlanAsync(Guid subscriptionId, Guid newTariffPlanId)
        {
            var response = await _httpClient.PatchAsync($"api/v1/Subscriptions/{subscriptionId}/change-tariff?newTariffPlanId={newTariffPlanId}", null);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.Error.WriteLine($"ChangeTariffPlan error ({response.StatusCode}): {error}");
            }
            return response.IsSuccessStatusCode;
        }

        public async Task<List<TariffPlanOption>> GetActiveTariffPlansByServiceAsync(Guid serviceId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/v1/TariffPlans/by-service/active/{serviceId}");

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return new List<TariffPlanOption>();
                }

                var result = await response.Content.ReadFromJsonAsync<List<TariffPlanOption>>();
                return result ?? new List<TariffPlanOption>();
            }
            catch (Exception ex)
            {
                return new List<TariffPlanOption>();
            }
        }
    }
}

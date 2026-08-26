using ClientOpsPortal.Application.DTOs;
using ClientOpsPortal.Application.Interfaces;
using ClientOpsPortal.Domain.Enums;
using System.Net.Http.Json;
using System.Text.Json;

namespace ClientOpsPortal.Infrastructure.Clients
{
    public class SubscriptionHistoryClient : ISubscriptionHistoryClient
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public SubscriptionHistoryClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task<SubscriptionHistoryDto?> GetHistoryByIdAsync(Guid id, CancellationToken ct = default)
        {
            var response = await _httpClient.GetAsync($"api/v1/subscriptionhistories/{id}", ct);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<SubscriptionHistoryDto>(_jsonOptions, ct);
        }

        public async Task<IReadOnlyCollection<SubscriptionHistoryDto>> GetHistoriesBySubscriptionAsync(Guid subscriptionId, CancellationToken ct = default)
        {
            var response = await _httpClient.GetAsync($"api/v1/subscriptionhistories/by-subscription/{subscriptionId}", ct);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<SubscriptionHistoryDto>>(_jsonOptions, ct);
            return result ?? Array.Empty<SubscriptionHistoryDto>();
        }

        public async Task<IReadOnlyCollection<SubscriptionHistoryFullDto>> GetHistoriesByAbonentAsync(Guid abonentId, CancellationToken ct = default)
        {
            var response = await _httpClient.GetAsync($"api/v1/subscriptionhistories/by-abonent/{abonentId}", ct);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<SubscriptionHistoryFullDto>>(_jsonOptions, ct);
            return result ?? Array.Empty<SubscriptionHistoryFullDto>();
        }

        public async Task<SubscriptionHistoryDto> CreateHistoryAsync(SubscriptionHistoryEventDto eventDto, CancellationToken ct = default)
        {
            // Преобразуем в DTO для микросервиса
            var createDto = new
            {
                subscriptionId = eventDto.SubscriptionId,
                actionType = eventDto.ActionType,
                status = eventDto.Status,
                tariffPlanId = eventDto.TariffPlanId,
                tariffPlanName = eventDto.TariffPlanName,
                serviceName = eventDto.ServiceName,
                contractNumber = eventDto.ContractNumber,
                abonentId = eventDto.AbonentId,
                startDate = eventDto.StartDate,
                steps = new List<object>() // Пустой список, шаги будут добавляться отдельно
            };

            var content = JsonContent.Create(createDto, options: _jsonOptions);
            var response = await _httpClient.PostAsync("api/v1/subscriptionhistories", content, ct);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<SubscriptionHistoryDto>(_jsonOptions, ct)
                ?? throw new InvalidOperationException("Failed to create subscription history");
        }

        public async Task UpdateHistoryStatusAsync(Guid historyId, SubscriptionActionStatus status, CancellationToken ct = default)
        {
            var updateDto = new { status = status };
            var content = JsonContent.Create(updateDto, options: _jsonOptions);
            var response = await _httpClient.PutAsync($"api/v1/subscriptionhistories/{historyId}", content, ct);
            response.EnsureSuccessStatusCode();
        }

        public async Task<SubscriptionHistoryStepDto> CreateStepAsync(CreateSubscriptionHistoryStepDto stepDto, CancellationToken ct = default)
        {
            var content = JsonContent.Create(stepDto, options: _jsonOptions);
            var response = await _httpClient.PostAsync("api/v1/subscriptionhistorystep", content, ct);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<SubscriptionHistoryStepDto>(_jsonOptions, ct)
                ?? throw new InvalidOperationException("Failed to create subscription history step");
        }

        public async Task<IReadOnlyCollection<SubscriptionHistoryStepDto>> GetStepsByHistoryAsync(Guid historyId, CancellationToken ct = default)
        {
            var response = await _httpClient.GetAsync($"api/v1/subscriptionhistorystep/by-history/{historyId}", ct);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<SubscriptionHistoryStepDto>>(_jsonOptions, ct);
            return result ?? Array.Empty<SubscriptionHistoryStepDto>();
        }
    }
}
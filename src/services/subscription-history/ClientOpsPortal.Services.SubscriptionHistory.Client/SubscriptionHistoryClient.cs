using ClientOpsPortal.Services.SubscriptionHistory.Contracts.DTOs;
using ClientOpsPortal.Services.SubscriptionHistory.Contracts.Models;
using System.Linq.Expressions;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

using SubscriptionHistoryModel = ClientOpsPortal.Services.SubscriptionHistory.Contracts.Models.SubscriptionHistory;

namespace ClientOpsPortal.Services.SubscriptionHistory.Client
{
    internal class SubscriptionHistoryClient : ISubscriptionhistoryClient
    {
        private readonly HttpClient _http;
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public SubscriptionHistoryClient(HttpClient http)
        {
            _http = http;
        }

        #region SubscriptionHistory

        public async Task<IReadOnlyCollection<SubscriptionHistoryDto>> GetAllSubscriptionHistoryAsync(
            bool withIncludes = false,
            CancellationToken ct = default)
        {
            var response = await _http.GetAsync(
                $"/api/v1/subscriptionhistories?withIncludes={withIncludes}",
                ct);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<SubscriptionHistoryDto>>(
                JsonOptions,
                ct);

            return result ?? Array.Empty<SubscriptionHistoryDto>();
        }

        public async Task<SubscriptionHistoryDto?> GetSubscriptionHistoryByIdAsync(
            Guid id,
            bool withIncludes = true,
            CancellationToken ct = default)
        {
            var response = await _http.GetAsync(
                $"/api/v1/subscriptionhistories/{id}?withIncludes={withIncludes}",
                ct);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<SubscriptionHistoryDto>(JsonOptions, ct);
        }

        public async Task<SubscriptionHistoryDto?> GetSubscriptionHistoryWhereAsync(
            Expression<Func<SubscriptionHistoryModel, bool>> predicate,
            Guid id,
            CancellationToken ct = default)
        {
            return await GetSubscriptionHistoryByIdAsync(id, true, ct);
        }

        public async Task<IReadOnlyCollection<SubscriptionHistoryDto>> GetHistoryBySubscriptionAsync(
            Guid subscriptionId,
            CancellationToken ct = default)
        {
            var response = await _http.GetAsync(
                $"/api/v1/subscriptionhistories/by-subscription/{subscriptionId}",
                ct);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<SubscriptionHistoryDto>>(
                JsonOptions,
                ct);

            return result ?? Array.Empty<SubscriptionHistoryDto>();
        }

        public async Task<IReadOnlyCollection<SubscriptionHistoryFullDto>> GetSubscriptionsHistoryByAbonentIdAsync(
            Guid abonentId,
            CancellationToken ct = default)
        {
            var response = await _http.GetAsync(
                $"/api/v1/subscriptionhistories/by-abonent/{abonentId}",
                ct);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<SubscriptionHistoryFullDto>>(
                JsonOptions,
                ct);

            return result ?? Array.Empty<SubscriptionHistoryFullDto>();
        }

        public async Task<SubscriptionHistoryDto> CreateSubscriptionHistoryAsync(
            CreateSubscriptionHistoryDto dto,
            CancellationToken ct = default)
        {
            var content = JsonContent.Create(dto, options: JsonOptions);
            var response = await _http.PostAsync("/api/v1/subscriptionhistories", content, ct);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<SubscriptionHistoryDto>(JsonOptions, ct)
                ?? throw new InvalidOperationException("Failed to deserialize created subscription history");
        }

        public async Task<SubscriptionHistoryDto> UpdateSubscriptionHistoryAsync(
            Guid id,
            UpdateSubscriptionHistoryDto dto,
            CancellationToken ct = default)
        {
            var content = JsonContent.Create(dto, options: JsonOptions);
            var response = await _http.PutAsync($"/api/v1/subscriptionhistories/{id}", content, ct);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                throw new KeyNotFoundException($"SubscriptionHistory with ID {id} not found");

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<SubscriptionHistoryDto>(JsonOptions, ct)
                ?? throw new InvalidOperationException("Failed to deserialize updated subscription history");
        }

        public async Task DeleteSubscriptionHistoryAsync(
            Guid id,
            CancellationToken ct = default)
        {
            var response = await _http.DeleteAsync($"/api/v1/subscriptionhistories/{id}", ct);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                throw new KeyNotFoundException($"SubscriptionHistory with ID {id} not found");

            response.EnsureSuccessStatusCode();
        }

        #endregion

        #region SubscriptionHistoryStep

        public async Task<IReadOnlyCollection<SubscriptionHistoryStepDto>> GetAllSubscriptionHistoryStepAsync(
            bool withIncludes = false,
            CancellationToken ct = default)
        {
            var response = await _http.GetAsync(
                $"/api/v1/subscriptionhistorystep?withIncludes={withIncludes}",
                ct);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<SubscriptionHistoryStepDto>>(
                JsonOptions,
                ct);

            return result ?? Array.Empty<SubscriptionHistoryStepDto>();
        }

        public async Task<SubscriptionHistoryStepDto?> GetSubscriptionHistoryStepByIdAsync(
            Guid id,
            bool withIncludes = true,
            CancellationToken ct = default)
        {
            var response = await _http.GetAsync(
                $"/api/v1/subscriptionhistorystep/{id}?withIncludes={withIncludes}",
                ct);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<SubscriptionHistoryStepDto>(JsonOptions, ct);
        }

        public async Task<SubscriptionHistoryStepDto?> GetSubscriptionHistoryStepWhereAsync(
            Expression<Func<SubscriptionHistoryStep, bool>> predicate,
            Guid id,
            CancellationToken ct = default)
        {
            return await GetSubscriptionHistoryStepByIdAsync(id, true, ct);
        }

        public async Task<IReadOnlyCollection<SubscriptionHistoryStepDto>> GetStepsByHistoryAsync(
            Guid historyId,
            CancellationToken ct = default)
        {
            var response = await _http.GetAsync(
                $"/api/v1/subscriptionhistorystep/by-history/{historyId}",
                ct);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<SubscriptionHistoryStepDto>>(
                JsonOptions,
                ct);

            return result ?? Array.Empty<SubscriptionHistoryStepDto>();
        }

        public async Task<SubscriptionHistoryStepDto> CreateSubscriptionHistoryStepAsync(
            CreateSubscriptionHistoryStepDto dto,
            CancellationToken ct = default)
        {
            var content = JsonContent.Create(dto, options: JsonOptions);
            var response = await _http.PostAsync("/api/v1/subscriptionhistorystep", content, ct);

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                throw new InvalidOperationException($"Failed to create step: {error}");
            }

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<SubscriptionHistoryStepDto>(JsonOptions, ct)
                ?? throw new InvalidOperationException("Failed to deserialize created step");
        }

        public async Task<SubscriptionHistoryStepDto> UpdateSubscriptionHistoryStepAsync(
            Guid id,
            UpdateSubscriptionHistoryStepDto dto,
            CancellationToken ct = default)
        {
            var content = JsonContent.Create(dto, options: JsonOptions);
            var response = await _http.PutAsync($"/api/v1/subscriptionhistorystep/{id}", content, ct);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                throw new KeyNotFoundException($"SubscriptionHistoryStep with ID {id} not found");

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<SubscriptionHistoryStepDto>(JsonOptions, ct)
                ?? throw new InvalidOperationException("Failed to deserialize updated step");
        }

        public async Task DeleteSubscriptionHistoryStepAsync(
            Guid id,
            CancellationToken ct = default)
        {
            var response = await _http.DeleteAsync($"/api/v1/subscriptionhistorystep/{id}", ct);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                throw new KeyNotFoundException($"SubscriptionHistoryStep with ID {id} not found");

            response.EnsureSuccessStatusCode();
        }

        #endregion
    }
}
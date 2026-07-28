using ClientOpsPortal.Services.Directory.Contracts.DTOs;
using System.Net.Http.Json;
using System.Text.Json;

namespace ClientOpsPortal.Services.Directory.Client
{
    public class ServicesDirectoryClient : IServicesDirectoryClient
    {
        private readonly HttpClient _http;
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ServicesDirectoryClient(HttpClient http)
        {
            _http = http;
        }

        // === Services ===

        public async Task<IReadOnlyCollection<ServiceDto>> GetAllServicesAsync(bool withIncludes = false, CancellationToken ct = default)
        {
            var result = await _http.GetFromJsonAsync<List<ServiceDto>>(
                $"api/v1/services?withIncludes={withIncludes.ToString().ToLower()}", JsonOptions, ct);
            return result ?? [];
        }

        public async Task<ServiceDto?> GetServiceByIdAsync(Guid id, bool withIncludes = true, CancellationToken ct = default)
        {
            return await _http.GetFromJsonAsync<ServiceDto>(
                $"api/v1/services/{id}?withIncludes={withIncludes.ToString().ToLower()}", JsonOptions, ct);
        }

        public async Task<ServiceFullDataDto?> GetFullServiceDataAsync(Guid id, CancellationToken ct = default)
        {
            return await _http.GetFromJsonAsync<ServiceFullDataDto>(
                $"api/v1/services/full/{id}", JsonOptions, ct);
        }

        public async Task<IReadOnlyCollection<ServiceShortDataDto>> GetActiveServicesAsync(CancellationToken ct = default)
        {
            var result = await _http.GetFromJsonAsync<List<ServiceShortDataDto>>(
                "api/v1/services/active", JsonOptions, ct);
            return result ?? [];
        }

        public async Task<ServiceDto> CreateServiceAsync(CreateServiceDto dto, CancellationToken ct = default)
        {
            var response = await _http.PostAsJsonAsync("api/v1/services", dto, ct);
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<ServiceDto>(JsonOptions, ct))!;
        }

        public async Task<ServiceDto> UpdateServiceAsync(Guid id, UpdateServiceDto dto, CancellationToken ct = default)
        {
            var response = await _http.PutAsJsonAsync($"api/v1/services/{id}", dto, ct);
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<ServiceDto>(JsonOptions, ct))!;
        }

        public async Task DeleteServiceAsync(Guid id, CancellationToken ct = default)
        {
            var response = await _http.DeleteAsync($"api/v1/services/{id}", ct);
            response.EnsureSuccessStatusCode();
        }

        // === TariffPlans ===

        public async Task<IReadOnlyCollection<TariffPlanDto>> GetAllTariffPlansAsync(bool withIncludes = false, CancellationToken ct = default)
        {
            var result = await _http.GetFromJsonAsync<List<TariffPlanDto>>(
                $"api/v1/tariffplans?withIncludes={withIncludes.ToString().ToLower()}", JsonOptions, ct);
            return result ?? [];
        }

        public async Task<TariffPlanDto?> GetTariffPlanByIdAsync(Guid id, bool withIncludes = true, CancellationToken ct = default)
        {
            return await _http.GetFromJsonAsync<TariffPlanDto>(
                $"api/v1/tariffplans/{id}?withIncludes={withIncludes.ToString().ToLower()}", JsonOptions, ct);
        }

        public async Task<IReadOnlyCollection<TariffPlanDto>> GetTariffPlansByServiceAsync(Guid serviceId, CancellationToken ct = default)
        {
            var result = await _http.GetFromJsonAsync<List<TariffPlanDto>>(
                $"api/v1/tariffplans/by-service/{serviceId}", JsonOptions, ct);
            return result ?? [];
        }

        public async Task<IReadOnlyCollection<TariffPlanShortDataDto>> GetActiveTariffPlansByServiceAsync(Guid serviceId, CancellationToken ct = default)
        {
            var result = await _http.GetFromJsonAsync<List<TariffPlanShortDataDto>>(
                $"api/v1/tariffplans/by-service/active/{serviceId}", JsonOptions, ct);
            return result ?? [];
        }

        public async Task<TariffPlanDto> CreateTariffPlanAsync(CreateTariffPlanDto dto, CancellationToken ct = default)
        {
            var response = await _http.PostAsJsonAsync("api/v1/tariffplans", dto, ct);
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<TariffPlanDto>(JsonOptions, ct))!;
        }

        public async Task<TariffPlanDto> UpdateTariffPlanAsync(Guid id, UpdateTariffPlanDto dto, CancellationToken ct = default)
        {
            var response = await _http.PutAsJsonAsync($"api/v1/tariffplans/{id}", dto, ct);
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<TariffPlanDto>(JsonOptions, ct))!;
        }

        public async Task DeleteTariffPlanAsync(Guid id, CancellationToken ct = default)
        {
            var response = await _http.DeleteAsync($"api/v1/tariffplans/{id}", ct);
            response.EnsureSuccessStatusCode();
        }
    }
}

using ClientOpsPortal.Services.Directory.Client;
using ClientOpsPortal.Services.Directory.Contracts.DTOs;
using Microsoft.Extensions.Caching.Memory;

namespace ClientOpsPortal.Api.Services
{
    public class DirectoryCacheService : ClientOpsPortal.Application.Interfaces.IDirectoryCacheService
    {
        private readonly IDirectoryGrpcClient _client;
        private readonly IMemoryCache _cache;

        private const string ServicesKey = "directory:services:all";
        private const string TariffsKey = "directory:tariffs:all";

        public DirectoryCacheService(IDirectoryGrpcClient client, IMemoryCache cache)
        {
            _client = client;
            _cache = cache;
        }

        public async Task<ServiceDto?> GetServiceAsync(Guid id, CancellationToken ct = default)
        {
            var services = await GetAllServicesAsync(ct);
            return services.FirstOrDefault(s => s.Id == id);
        }

        public async Task<TariffPlanDto?> GetTariffPlanAsync(Guid id, CancellationToken ct = default)
        {
            var tariffs = await GetAllTariffPlansAsync(ct);
            return tariffs.FirstOrDefault(t => t.Id == id);
        }

        public async Task<IReadOnlyCollection<ServiceDto>> GetAllServicesAsync(CancellationToken ct = default)
        {
            if (_cache.TryGetValue(ServicesKey, out List<ServiceDto>? cached) && cached != null)
                return cached;

            var services = await _client.GetAllServicesAsync(false, ct);

            var entryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(30))
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(60));

            _cache.Set(ServicesKey, services.ToList(), entryOptions);
            return services;
        }

        public async Task<IReadOnlyCollection<TariffPlanDto>> GetAllTariffPlansAsync(CancellationToken ct = default)
        {
            if (_cache.TryGetValue(TariffsKey, out List<TariffPlanDto>? cached) && cached != null)
                return cached;

            var tariffs = await _client.GetAllTariffPlansAsync(false, ct);

            var entryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(30))
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(60));

            _cache.Set(TariffsKey, tariffs.ToList(), entryOptions);
            return tariffs;
        }
    }
}

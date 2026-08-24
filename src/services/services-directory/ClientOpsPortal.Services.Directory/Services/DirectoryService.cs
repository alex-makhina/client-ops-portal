using ClientOpsPortal.Services.Directory.Contracts.DTOs;
using ClientOpsPortal.Services.Directory.Contracts.Exceptions;
using ClientOpsPortal.Services.Directory.Contracts.Models;
using ClientOpsPortal.Services.Directory.Data;
using ClientOpsPortal.Services.Reporting.Contracts.Events;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace ClientOpsPortal.Services.Directory.Services
{
    public class DirectoryService
    {
        private readonly ServiceRepository _serviceRepository;
        private readonly GenericRepository<TariffPlan> _tariffPlanRepository;
        private readonly IDistributedCache _cache;
        private readonly ServiceCacheOptions _options;
        private readonly IPublishEndpoint _publishEndpoint;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private const string ActiveServicesKey = "directory:services:active";
        private const string ActiveTariffsPrefix = "directory:tariffs:active:";
        private static string ServiceByIdKey(Guid id) => $"directory:service:{id}";
        private static string TariffByIdKey(Guid id) => $"directory:tariff:{id}";
        private static string ServiceFullKey(Guid id) => $"directory:service:full:{id}";

        public DirectoryService(
            ServiceRepository serviceRepository,
            GenericRepository<TariffPlan> tariffPlanRepository,
            IDistributedCache cache,
            IOptions<ServiceCacheOptions> options,
            IPublishEndpoint publishEndpoint)
        {
            _serviceRepository = serviceRepository;
            _tariffPlanRepository = tariffPlanRepository;
            _cache = cache;
            _options = options.Value;
            _publishEndpoint = publishEndpoint;
        }

        // === Services ===

        public async Task<IReadOnlyCollection<ServiceDto>> GetAllServicesAsync(bool withIncludes, CancellationToken ct)
        {
            var services = await _serviceRepository.GetAllAsync(withIncludes, ct);
            return services.Select(ToServiceDto).ToList();
        }

        public async Task<ServiceDto?> GetServiceByIdAsync(Guid id, bool withIncludes, CancellationToken ct)
        {
            var cacheKey = ServiceByIdKey(id);
            var cached = await _cache.GetStringAsync(cacheKey, ct);
            if (cached != null)
                return JsonSerializer.Deserialize<ServiceDto>(cached, JsonOptions);

            var service = await _serviceRepository.GetByIdAsync(id, withIncludes, ct);
            if (service == null)
                return null;

            var dto = ToServiceDto(service);
            await CacheAsync(cacheKey, dto, _options.ServiceByIdMinutes, ct);
            return dto;
        }

        public async Task<ServiceFullDataDto?> GetFullServiceDataAsync(Guid id, CancellationToken ct)
        {
            var cacheKey = ServiceFullKey(id);
            var cached = await _cache.GetStringAsync(cacheKey, ct);
            if (cached != null)
                return JsonSerializer.Deserialize<ServiceFullDataDto>(cached, JsonOptions);

            var service = await _serviceRepository.GetByIdAsync(id, true, ct);
            if (service == null)
                return null;

            var dto = ToServiceFullDataDto(service);
            await CacheAsync(cacheKey, dto, _options.ServiceByIdMinutes, ct);
            return dto;
        }

        public async Task<IReadOnlyCollection<ServiceShortDataDto>> GetActiveServicesAsync(CancellationToken ct)
        {
            var cached = await _cache.GetStringAsync(ActiveServicesKey, ct);
            if (cached != null)
                return JsonSerializer.Deserialize<List<ServiceShortDataDto>>(cached, JsonOptions)!;

            var now = DateTimeOffset.UtcNow;
            var services = await _serviceRepository.GetWhereAsync(
                s => s.EndDate == null || s.EndDate > now, false, ct);

            var result = services.Select(ToServiceShortDataDto).ToList();
            await CacheAsync(ActiveServicesKey, result, _options.ActiveServicesMinutes, ct);
            return result;
        }

        public async Task<ServiceDto> CreateServiceAsync(CreateServiceDto createDto, CancellationToken ct)
        {
            if (!await IsServiceNameUniqueAsync(createDto.Name, null, ct))
                throw new InvalidOperationException($"Услуга с названием '{createDto.Name}' уже существует");

            var service = ToEntity(createDto);
            await _serviceRepository.AddAsync(service, ct);

            var tariffSnapshots = service.TariffPlans.Select(tp => new TariffPlanSnapshot(
                tp.Id, tp.Name, tp.Description, tp.Price, tp.ServiceId, tp.BeginDate, tp.EndDate
            )).ToList();

            await _publishEndpoint.Publish(new ServiceCreatedEvent(
                service.Id, service.Name, service.Description, service.BeginDate, service.EndDate,
                service.CreatedAt, service.CreatedBy, service.UpdatedAt, service.UpdatedBy,
                tariffSnapshots,
                DateTimeOffset.UtcNow
            ), ct);

            await InvalidateServiceCachesAsync(ct);

            return ToServiceDto(service);
        }

        public async Task<ServiceDto> UpdateServiceAsync(Guid id, UpdateServiceDto updateDto, CancellationToken ct)
        {
            var service = await _serviceRepository.GetByIdAsync(id, false, ct)
                ?? throw new EntityNotFoundException(nameof(Service), id);

            if (!string.IsNullOrWhiteSpace(updateDto.Name) && service.Name != updateDto.Name)
                if (!await IsServiceNameUniqueAsync(updateDto.Name, id, ct))
                    throw new InvalidOperationException($"Услуга с названием '{updateDto.Name}' уже существует");

            if (!string.IsNullOrWhiteSpace(updateDto.Name))
                service.Name = updateDto.Name;
            if (!string.IsNullOrWhiteSpace(updateDto.Description))
                service.Description = updateDto.Description;
            if (updateDto.EndDate.HasValue)
                service.EndDate = updateDto.EndDate;
            service.BeginDate = updateDto.BeginDate;

            await _serviceRepository.UpdateAsync(service, ct);

            if (updateDto.TariffPlans != null)
                await UpdateTariffPlansAsync(id, updateDto.TariffPlans, ct);

            var updatedService = await _serviceRepository.GetByIdAsync(id, true, ct)
                ?? throw new EntityNotFoundException(nameof(Service), id);

            var tariffSnapshots = updatedService.TariffPlans.Select(tp => new TariffPlanSnapshot(
                tp.Id, tp.Name, tp.Description, tp.Price, tp.ServiceId, tp.BeginDate, tp.EndDate
            )).ToList();

            await _publishEndpoint.Publish(new ServiceUpdatedEvent(
                updatedService.Id, updatedService.Name, updatedService.Description,
                updatedService.BeginDate, updatedService.EndDate, updatedService.CreatedAt,
                updatedService.CreatedBy, updatedService.UpdatedAt, updatedService.UpdatedBy,
                tariffSnapshots,
                DateTimeOffset.UtcNow
            ), ct);

            await InvalidateServiceCachesAsync(ct);

            return ToServiceDto(service);
        }

        public async Task DeleteServiceAsync(Guid id, CancellationToken ct)
        {
            await _serviceRepository.DeleteAsync(id, ct);

            await _publishEndpoint.Publish(new ServiceDeletedEvent(id, DateTimeOffset.UtcNow), ct);
            
            await InvalidateServiceCachesAsync(ct);
        }

        // === TariffPlans ===

        public async Task<IReadOnlyCollection<TariffPlanDto>> GetAllTariffPlansAsync(bool withIncludes, CancellationToken ct)
        {
            var tariffs = await _tariffPlanRepository.GetAllAsync(withIncludes, ct);
            return tariffs.Select(ToTariffPlanDto).ToList();
        }

        public async Task<TariffPlanDto?> GetTariffPlanByIdAsync(Guid id, bool withIncludes, CancellationToken ct)
        {
            var cacheKey = TariffByIdKey(id);
            var cached = await _cache.GetStringAsync(cacheKey, ct);
            if (cached != null)
                return JsonSerializer.Deserialize<TariffPlanDto>(cached, JsonOptions);

            var tariff = await _tariffPlanRepository.GetByIdAsync(id, withIncludes, ct);
            if (tariff == null)
                return null;

            var dto = ToTariffPlanDto(tariff);
            await CacheAsync(cacheKey, dto, _options.ServiceByIdMinutes, ct);
            return dto;
        }

        public async Task<IReadOnlyCollection<TariffPlanDto>> GetTariffPlansByServiceAsync(Guid serviceId, CancellationToken ct)
        {
            var tariffs = await _tariffPlanRepository.GetWhereAsync(t => t.ServiceId == serviceId, false, ct);
            return tariffs.Select(ToTariffPlanDto).ToList();
        }

        public async Task<IReadOnlyCollection<TariffPlanShortDataDto>> GetActiveTariffPlansByServiceAsync(Guid serviceId, CancellationToken ct)
        {
            var cacheKey = $"{ActiveTariffsPrefix}{serviceId}";
            var cached = await _cache.GetStringAsync(cacheKey, ct);
            if (cached != null)
                return JsonSerializer.Deserialize<List<TariffPlanShortDataDto>>(cached, JsonOptions)!;

            var now = DateTimeOffset.UtcNow;
            var tariffs = await _tariffPlanRepository.GetWhereAsync(
                t => t.ServiceId == serviceId && (t.EndDate == null || t.EndDate > now), false, ct);

            var result = tariffs.Select(ToTariffPlanShortDataDto).ToList();
            await CacheAsync(cacheKey, result, _options.ActiveServicesMinutes, ct);
            return result;
        }

        public async Task<TariffPlanDto> CreateTariffPlanAsync(CreateTariffPlanDto createDto, CancellationToken ct)
        {
            var tariff = ToEntity(createDto);
            await _tariffPlanRepository.AddAsync(tariff, ct);

            await _publishEndpoint.Publish(new TariffPlanCreatedEvent(
                tariff.Id, tariff.Name, tariff.Description, tariff.Price, tariff.ServiceId,
                tariff.BeginDate, tariff.EndDate, DateTimeOffset.UtcNow
            ), ct);

            await InvalidateTariffCachesAsync(tariff.ServiceId, ct);
            return ToTariffPlanDto(tariff);
        }

        public async Task<TariffPlanDto> UpdateTariffPlanAsync(Guid id, UpdateTariffPlanDto updateDto, CancellationToken ct)
        {
            var tariff = await _tariffPlanRepository.GetByIdAsync(id, false, ct)
                ?? throw new EntityNotFoundException(nameof(TariffPlan), id);

            UpdateEntityPartial(updateDto, tariff);
            await _tariffPlanRepository.UpdateAsync(tariff, ct);

            await _publishEndpoint.Publish(new TariffPlanUpdatedEvent(
                tariff.Id, tariff.Name, tariff.Description, tariff.Price, tariff.ServiceId,
                tariff.BeginDate, tariff.EndDate, DateTimeOffset.UtcNow
            ), ct);

            await InvalidateTariffCachesAsync(tariff.ServiceId, ct);

            return ToTariffPlanDto(tariff);
        }

        public async Task DeleteTariffPlanAsync(Guid id, CancellationToken ct)
        {
            var tariff = await _tariffPlanRepository.GetByIdAsync(id, false, ct);
            await _tariffPlanRepository.DeleteAsync(id, ct);

            await _publishEndpoint.Publish(new TariffPlanDeletedEvent(id, DateTimeOffset.UtcNow), ct);

            if (tariff != null)
                await InvalidateTariffCachesAsync(tariff.ServiceId, ct);
        }

        public async Task<bool> IsServiceNameUniqueAsync(string name, Guid? excludeId = null, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            var services = await _serviceRepository.GetWhereAsync(
                s => s.Name.ToLower() == name.ToLower(), false, ct);

            if (excludeId.HasValue)
                return !services.Any(s => s.Id != excludeId.Value);

            return !services.Any();
        }

        // === Cache helpers ===

        private async Task CacheAsync<T>(string key, T value, int minutes, CancellationToken ct)
        {
            var options = new DistributedCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(minutes)
            };
            await _cache.SetStringAsync(key, JsonSerializer.Serialize(value, JsonOptions), options, ct);
        }

        private async Task InvalidateServiceCachesAsync(CancellationToken ct)
        {
            await _cache.RemoveAsync(ActiveServicesKey, ct);
        }

        private async Task InvalidateTariffCachesAsync(Guid serviceId, CancellationToken ct)
        {
            await _cache.RemoveAsync($"{ActiveTariffsPrefix}{serviceId}", ct);
            await _cache.RemoveAsync(ActiveServicesKey, ct);
        }

        // === Mappers ===

        private static ServiceDto ToServiceDto(Service s) => new()
        {
            Id = s.Id, Name = s.Name, Description = s.Description,
            BeginDate = s.BeginDate, EndDate = s.EndDate,
            CreatedAt = s.CreatedAt, CreatedBy = s.CreatedBy,
            UpdatedAt = s.UpdatedAt, UpdatedBy = s.UpdatedBy
        };

        private static ServiceShortDataDto ToServiceShortDataDto(Service s) => new()
        {
            Id = s.Id, Name = s.Name, Description = s.Description,
            BeginDate = s.BeginDate, EndDate = s.EndDate
        };

        private static ServiceFullDataDto ToServiceFullDataDto(Service s) => new()
        {
            Id = s.Id, Name = s.Name, Description = s.Description,
            BeginDate = s.BeginDate, EndDate = s.EndDate,
            TariffPlans = s.TariffPlans?.Select(ToTariffPlanDto).ToList() ?? []
        };

        private static TariffPlanDto ToTariffPlanDto(TariffPlan t) => new()
        {
            Id = t.Id, Name = t.Name, Description = t.Description,
            Price = t.Price, ServiceId = t.ServiceId,
            BeginDate = t.BeginDate, EndDate = t.EndDate
        };

        private static TariffPlanShortDataDto ToTariffPlanShortDataDto(TariffPlan t) => new()
        {
            Id = t.Id, Name = t.Name, Price = t.Price
        };

        private static Service ToEntity(CreateServiceDto dto)
        {
            var serviceId = Guid.NewGuid();
            var service = new Service
            {
                Id = serviceId, Name = dto.Name, Description = dto.Description,
                BeginDate = dto.BeginDate.ToUniversalTime(),
                EndDate = dto.EndDate?.ToUniversalTime(),
                TariffPlans = []
            };

            if (dto.TariffPlans != null)
            {
                foreach (var td in dto.TariffPlans)
                {
                    service.TariffPlans.Add(new TariffPlan
                    {
                        Id = Guid.NewGuid(), Name = td.Name, Description = td.Description,
                        Price = td.Price, ServiceId = serviceId,
                        BeginDate = td.BeginDate.ToUniversalTime(),
                        EndDate = td.EndDate?.ToUniversalTime()
                    });
                }
            }
            return service;
        }

        private static TariffPlan ToEntity(CreateTariffPlanDto dto) => new()
        {
            Id = Guid.NewGuid(), Name = dto.Name, Description = dto.Description,
            Price = dto.Price, ServiceId = dto.ServiceId,
            BeginDate = dto.BeginDate, EndDate = dto.EndDate
        };

        private static void UpdateEntityPartial(UpdateTariffPlanDto dto, TariffPlan entity)
        {
            if (!string.IsNullOrWhiteSpace(dto.Name))
                entity.Name = dto.Name;
            if (!string.IsNullOrWhiteSpace(dto.Description))
                entity.Description = dto.Description;
            if (dto.Price.HasValue && dto.Price.Value > 0)
                entity.Price = dto.Price.Value;
            if (dto.BeginDate.HasValue && dto.BeginDate.Value != default)
                entity.BeginDate = dto.BeginDate.Value;
            if (dto.EndDate.HasValue)
                entity.EndDate = dto.EndDate;
        }

        private async Task UpdateTariffPlansAsync(Guid serviceId, List<UpdateTariffPlanFromServiceDto> items, CancellationToken ct)
        {
            var existing = await _tariffPlanRepository.GetWhereAsync(t => t.ServiceId == serviceId, false, ct);
            var existingDict = existing.ToDictionary(t => t.Id);

            var toAdd = items.Where(t => t.Id == Guid.Empty).ToList();
            var toUpdate = items.Where(t => t.Id != Guid.Empty && existingDict.ContainsKey(t.Id)).ToList();
            var toDelete = existing.Select(t => t.Id).Where(id => !items.Any(t => t.Id == id)).ToList();

            foreach (var id in toDelete)
                await _tariffPlanRepository.DeleteAsync(id, ct);

            foreach (var item in toUpdate)
            {
                var entity = existingDict[item.Id];
                entity.Name = item.Name;
                entity.Description = item.Description;
                entity.Price = item.Price;
                entity.BeginDate = item.BeginDate;
                entity.EndDate = item.EndDate;
                await _tariffPlanRepository.UpdateAsync(entity, ct);
            }

            foreach (var item in toAdd)
            {
                await _tariffPlanRepository.AddAsync(new TariffPlan
                {
                    Id = Guid.NewGuid(), Name = item.Name, Description = item.Description,
                    Price = item.Price, ServiceId = serviceId,
                    BeginDate = item.BeginDate, EndDate = item.EndDate
                }, ct);
            }
        }
    }
}

using ClientOpsPortal.Application.DTOs;
using ClientOpsPortal.Application.Interfaces;
using ClientOpsPortal.Application.Mappings;
using ClientOpsPortal.Domain.Entities;
using ClientOpsPortal.Domain.Exceptions;
using ClientOpsPortal.Domain.Interfaces.Repositories;
using System.Linq.Expressions;

namespace ClientOpsPortal.Application.Services
{
    public class ServiceService : IServiceService
    {
        private readonly IGenericRepository<Service> _serviceRepository;
        private readonly IGenericRepository<TariffPlan> _tariffPlanRepository;

        public ServiceService(
            IGenericRepository<Service> serviceRepository,
            IGenericRepository<TariffPlan> tariffPlanRepository)
        {
            _serviceRepository = serviceRepository;
            _tariffPlanRepository = tariffPlanRepository;
        }

        public async Task<ServiceDto?> GetByIdAsync(Guid id, bool withIncludes = false, CancellationToken ct = default)
        {
            var service = await _serviceRepository.GetByIdAsync(id, withIncludes, ct);
            return service?.ToServiceDto();
        }

        public async Task<IReadOnlyCollection<ServiceDto>> GetAllAsync(bool withIncludes = false, CancellationToken ct = default)
        {
            var services = await _serviceRepository.GetAllAsync(withIncludes, ct);
            return services.Select(s => s.ToServiceDto()).ToList();
        }

        public async Task<ServiceDto> CreateAsync(CreateServiceDto createDto, CancellationToken ct = default)
        {
            var service = createDto.ToEntity();
            await _serviceRepository.AddAsync(service, ct);

            return service.ToServiceDto();
        }

        public async Task<ServiceDto> UpdateAsync(Guid id, UpdateServiceDto updateDto, CancellationToken ct = default)
        {
            var service = await _serviceRepository.GetByIdAsync(id, false, ct);
            if (service == null)
                throw new EntityNotFoundException(typeof(Service), id);

            if (!string.IsNullOrWhiteSpace(updateDto.Name))
                service.Name = updateDto.Name;

            if (!string.IsNullOrWhiteSpace(updateDto.Description))
                service.Description = updateDto.Description;

            if (updateDto.EndDate.HasValue)
                service.EndDate = updateDto.EndDate;

            await _serviceRepository.UpdateAsync(service, ct);

            if (updateDto.TariffPlans != null)
                await UpdateTariffPlansAsync(id, updateDto.TariffPlans, ct);

            return service.ToServiceDto();
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            await _serviceRepository.DeleteAsync(id, ct);
        }

        public async Task<IReadOnlyCollection<ServiceDto>> GetWhereAsync(Expression<Func<Service, bool>> predicate, bool withIncludes = false, CancellationToken ct = default)
        {
            var services = await _serviceRepository.GetWhereAsync(predicate, withIncludes, ct);
            return services.Select(s => s.ToServiceDto()).ToList();
        }

        public async Task<ServiceFullDataDto?> GetFullServiceDataAsync(Guid serviceId, CancellationToken ct = default)
        {
            var service = await _serviceRepository.GetByIdAsync(serviceId, true, ct);
            return service?.ToServiceFullDataDto();
        }

        public async Task<IReadOnlyCollection<ServiceShortDataDto>> GetActiveServicesAsync(CancellationToken ct = default)
        {
            var now = DateTimeOffset.UtcNow;
            var services = await _serviceRepository.GetWhereAsync(s => s.EndDate == null || s.EndDate > now, false, ct);
            return services.Select(s => s.ToServiceShortDataDto()).ToList();
        }

        private async Task UpdateTariffPlansAsync(Guid serviceId, List<UpdateTariffPlanFromServiceDto> tariffPlanItems, CancellationToken ct = default)
        {
            // Получаем существующие тарифные планы услуги
            var existingTariffs = await _tariffPlanRepository.GetWhereAsync(t => t.ServiceId == serviceId, false, ct);
            var existingTariffDict = existingTariffs.ToDictionary(t => t.Id);

            // Разделяем элементы на добавляемые, обновляемые и удаляемые
            var itemsToAdd = tariffPlanItems.Where(t => t.Id == Guid.Empty).ToList();
            var itemsToUpdate = tariffPlanItems.Where(t => t.Id != Guid.Empty && existingTariffDict.ContainsKey(t.Id)).ToList();
            var idsToDelete = existingTariffs.Select(t => t.Id)
                .Where(id => !tariffPlanItems.Any(t => t.Id == id))
                .ToList();

            // Удаляем тарифные планы
            foreach (var id in idsToDelete)
            {
                await _tariffPlanRepository.DeleteAsync(id, ct);
            }

            // Обновляем существующие тарифные планы
            foreach (var item in itemsToUpdate)
            {
                var existingTariff = existingTariffDict[item.Id];
                item.UpdateEntityFull(existingTariff);
                await _tariffPlanRepository.UpdateAsync(existingTariff, ct);
            }

            // Добавляем новые тарифные планы
            foreach (var item in itemsToAdd)
            {
                var newTariff = item.ToEntityFromUpdateItem(serviceId);
                await _tariffPlanRepository.AddAsync(newTariff, ct);
            }
        }
    }
}
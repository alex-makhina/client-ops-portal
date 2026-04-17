using ClientOpsPortal.Application.DTOs;
using ClientOpsPortal.Application.Interfaces;
using ClientOpsPortal.Application.Mappings;
using ClientOpsPortal.Domain.Entities;
using ClientOpsPortal.Domain.Exceptions;
using ClientOpsPortal.Domain.Interfaces.Repositories;
using System.Linq.Expressions;

namespace ClientOpsPortal.Application.Services
{
    public class TariffPlanService : ITariffPlanService
    {
        private readonly IGenericRepository<TariffPlan> _tariffPlanRepository;

        public TariffPlanService(IGenericRepository<TariffPlan> tariffPlanRepository)
        {
            _tariffPlanRepository = tariffPlanRepository;
        }

        public async Task<TariffPlanDto?> GetByIdAsync(Guid id, bool withIncludes = false, CancellationToken ct = default)
        {
            var tariffPlan = await _tariffPlanRepository.GetByIdAsync(id, withIncludes, ct);
            return tariffPlan?.ToTariffPlanDto();
        }

        public async Task<IReadOnlyCollection<TariffPlanDto>> GetAllAsync(bool withIncludes = false, CancellationToken ct = default)
        {
            var tariffPlans = await _tariffPlanRepository.GetAllAsync(withIncludes, ct);
            return tariffPlans.Select(tp => tp.ToTariffPlanDto()).ToList();
        }

        public async Task<TariffPlanDto> CreateAsync(CreateTariffPlanDto createDto, CancellationToken ct = default)
        {
            var tariffPlan = createDto.ToEntity();
            await _tariffPlanRepository.AddAsync(tariffPlan, ct);
            return tariffPlan.ToTariffPlanDto();
        }

        public async Task<TariffPlanDto> UpdateAsync(Guid id, UpdateTariffPlanDto updateDto, CancellationToken ct = default)
        {
            var tariffPlan = await _tariffPlanRepository.GetByIdAsync(id, false, ct);
            if (tariffPlan == null)
                throw new EntityNotFoundException(typeof(TariffPlan), id);

            updateDto.UpdateEntityPartial(tariffPlan);
            await _tariffPlanRepository.UpdateAsync(tariffPlan, ct);

            return tariffPlan.ToTariffPlanDto();
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            await _tariffPlanRepository.DeleteAsync(id, ct);
        }

        public async Task<IReadOnlyCollection<TariffPlanDto>> GetWhereAsync(Expression<Func<TariffPlan, bool>> predicate, bool withIncludes = false, CancellationToken ct = default)
        {
            var tariffPlans = await _tariffPlanRepository.GetWhereAsync(predicate, withIncludes, ct);
            return tariffPlans.Select(tp => tp.ToTariffPlanDto()).ToList();
        }

        public async Task<IReadOnlyCollection<TariffPlanShortDataDto>> GetActiveTariffPlansByServiceAsync(Guid serviceId, CancellationToken ct = default)
        {
            var now = DateTimeOffset.UtcNow;
            var tariffs = await _tariffPlanRepository.GetWhereAsync(
                t => t.ServiceId == serviceId && (t.EndDate == null || t.EndDate > now),
                false, ct);
            return tariffs.Select(t => t.ToTariffPlanShortDataDto()).ToList();
        }

        public async Task<IReadOnlyCollection<TariffPlanDto>> GetTariffPlansByServiceAsync(Guid serviceId, CancellationToken ct = default)
        {
            var tariffs = await _tariffPlanRepository.GetWhereAsync(t => t.ServiceId == serviceId, false, ct);
            return tariffs.Select(t => t.ToTariffPlanDto()).ToList();
        }
    }
}
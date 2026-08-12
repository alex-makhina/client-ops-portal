using ClientOpsPortal.Application.DTOs;
using ClientOpsPortal.Application.Interfaces;
using ClientOpsPortal.Application.Mappings;
using ClientOpsPortal.Domain.Entities;
using ClientOpsPortal.Domain.Enums;
using ClientOpsPortal.Domain.Exceptions;
using ClientOpsPortal.Domain.Interfaces.Repositories;
using System.Linq.Expressions;

namespace ClientOpsPortal.Application.Services
{
    public class SubscriptionHistoryStepService : ISubscriptionHistoryStepService
    {
        private readonly IGenericRepository<SubscriptionHistoryStep> _stepRepository;
        private readonly IGenericRepository<SubscriptionHistory> _historyRepository;

        public SubscriptionHistoryStepService(
            IGenericRepository<SubscriptionHistoryStep> stepRepository,
            IGenericRepository<SubscriptionHistory> historyRepository)
        {
            _stepRepository = stepRepository;
            _historyRepository = historyRepository;
        }

        public async Task<SubscriptionHistoryStepDto?> GetByIdAsync(Guid id, bool withIncludes = false, CancellationToken ct = default)
        {
            var history = await _stepRepository.GetByIdAsync(id, withIncludes, ct);
            return history?.ToSubscriptionHistoryStepDto();
        }

        public async Task<IReadOnlyCollection<SubscriptionHistoryStepDto>> GetAllAsync(bool withIncludes = false, CancellationToken ct = default)
        {
            var histories = await _stepRepository.GetAllAsync(withIncludes, ct);
            return histories.Select(h => h.ToSubscriptionHistoryStepDto()).ToList();
        }

        public async Task<IReadOnlyCollection<SubscriptionHistoryStepDto>> GetWhereAsync( Expression<Func<SubscriptionHistoryStep, bool>> predicate, bool withIncludes = false,CancellationToken ct = default)
        {
            var steps = await _stepRepository.GetWhereAsync(predicate, withIncludes, ct);
            return steps.Select(s => s.ToSubscriptionHistoryStepDto()).ToList();
        }

        public async Task<SubscriptionHistoryStepDto> CreateAsync(CreateSubscriptionHistoryStepDto createDto, CancellationToken ct = default)
        {
            var step = createDto.ToEntity();
            await _stepRepository.AddAsync(step, ct);
            return step.ToSubscriptionHistoryStepDto();
        }

        public async Task<SubscriptionHistoryStepDto> UpdateAsync(Guid id, UpdateSubscriptionHistoryStepDto updateDto, CancellationToken ct = default)
        {
            var step = await _stepRepository.GetByIdAsync(id, false, ct);
            if (step == null)
                throw new EntityNotFoundException(typeof(SubscriptionHistory), id);

            updateDto.UpdateEntity(step);
            await _stepRepository.UpdateAsync(step, ct);
            return step.ToSubscriptionHistoryStepDto();
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            await _stepRepository.DeleteAsync(id, ct);
        }

        public async Task<IReadOnlyCollection<SubscriptionHistoryStepDto>> GetStepsByHistoryAsync(Guid historyId, CancellationToken ct = default)
        {
            var steps = await _stepRepository.GetWhereAsync(s => s.SubscriptionHistoryId == historyId, false, ct);
            return steps.Select(s => s.ToSubscriptionHistoryStepDto()).ToList();
        }
    }
}
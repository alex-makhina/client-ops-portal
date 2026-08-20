using ClientOpsPortal.Services.SubscriptionHistory.Contracts.DTOs;
using ClientOpsPortal.Services.SubscriptionHistory.Contracts.Exceptions;
using ClientOpsPortal.Services.SubscriptionHistory.Contracts.Models;
using ClientOpsPortal.Services.SubscriptionHistory.Data;
using ClientOpsPortal.Services.SubscriptionHistory.Mappings;
using System.Linq.Expressions;
using System.Text.Json;

using SubscriptionHistoryModel = ClientOpsPortal.Services.SubscriptionHistory.Contracts.Models.SubscriptionHistory;

namespace ClientOpsPortal.Services.SubscriptionHistory.Services
{
    public class SubscriptionHistoryStepService
    {
        private readonly MongoRepository<SubscriptionHistoryStep> _subscriptionHistoryStepRepository;

        public SubscriptionHistoryStepService(MongoRepository<SubscriptionHistoryStep> subscriptionHistoryStepRepository)
        {
            _subscriptionHistoryStepRepository = subscriptionHistoryStepRepository;
        }

        public async Task<SubscriptionHistoryStepDto?> GetSubscriptionHistoryStepByIdAsync(Guid id, CancellationToken ct = default)
        {
            var history = await _subscriptionHistoryStepRepository.GetByIdAsync(id, ct);
            return history?.ToSubscriptionHistoryStepDto();
        }

        public async Task<IReadOnlyCollection<SubscriptionHistoryStepDto>> GetAllSubscriptionHistoryStepAsync(CancellationToken ct = default)
        {
            var histories = await _subscriptionHistoryStepRepository.GetAllAsync(ct);
            return histories.Select(h => h.ToSubscriptionHistoryStepDto()).ToList();
        }

        public async Task<IReadOnlyCollection<SubscriptionHistoryStepDto>> GetSubscriptionHistoryStepWhereAsync(Expression<Func<SubscriptionHistoryStep, bool>> predicate, CancellationToken ct = default)
        {
            var steps = await _subscriptionHistoryStepRepository.GetWhereAsync(predicate, ct);
            return steps.Select(s => s.ToSubscriptionHistoryStepDto()).ToList();
        }

        public async Task<SubscriptionHistoryStepDto> CreateSubscriptionHistoryStepAsync(CreateSubscriptionHistoryStepDto createDto, CancellationToken ct = default)
        {
            var step = createDto.ToEntity();
            await _subscriptionHistoryStepRepository.AddAsync(step, ct);
            return step.ToSubscriptionHistoryStepDto();
        }

        public async Task<SubscriptionHistoryStepDto> UpdateSubscriptionHistoryStepAsync(Guid id, UpdateSubscriptionHistoryStepDto updateDto, CancellationToken ct = default)
        {
            var step = await _subscriptionHistoryStepRepository.GetByIdAsync(id, ct);
            if (step == null)
                throw new EntityNotFoundException(typeof(SubscriptionHistoryModel), id);

            updateDto.UpdateEntity(step);
            await _subscriptionHistoryStepRepository.UpdateAsync(step, ct);
            return step.ToSubscriptionHistoryStepDto();
        }

        public async Task DeleteSubscriptionHistoryStepAsync(Guid id, CancellationToken ct = default)
        {
            await _subscriptionHistoryStepRepository.DeleteAsync(id, ct);
        }

        public async Task<IReadOnlyCollection<SubscriptionHistoryStepDto>> GetStepsByHistoryAsync(Guid historyId, CancellationToken ct = default)
        {
            var steps = await _subscriptionHistoryStepRepository.GetWhereAsync(s => s.SubscriptionHistoryId == historyId, ct);
            return steps.OrderBy(s => s.CreatedAt).Select(s => s.ToSubscriptionHistoryStepDto()).ToList();
        }
    }
}

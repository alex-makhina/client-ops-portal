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
    public class SubscriptionHistoryService : ISubscriptionHistoryService
    {
        private readonly IGenericRepository<SubscriptionHistory> _historyRepository;
        private readonly IGenericRepository<SubscriptionHistoryStep> _stepRepository;
        private readonly IGenericRepository<Subscription> _subscriptionRepository;

        public SubscriptionHistoryService(
            IGenericRepository<SubscriptionHistory> historyRepository,
            IGenericRepository<SubscriptionHistoryStep> stepRepository,
            IGenericRepository<Subscription> subscriptionRepository)
        {
            _historyRepository = historyRepository;
            _stepRepository = stepRepository;
            _subscriptionRepository = subscriptionRepository;
        }

        public async Task<SubscriptionHistoryDto?> GetByIdAsync(Guid id, bool withIncludes = false, CancellationToken ct = default)
        {
            var history = await _historyRepository.GetByIdAsync(id, withIncludes, ct);
            return history?.ToSubscriptionHistoryDto();
        }

        public async Task<IReadOnlyCollection<SubscriptionHistoryDto>> GetAllAsync(bool withIncludes = false, CancellationToken ct = default)
        {
            var histories = await _historyRepository.GetAllAsync(withIncludes, ct);
            return histories.Select(h => h.ToSubscriptionHistoryDto()).ToList();
        }

        public async Task<SubscriptionHistoryDto> CreateAsync(CreateSubscriptionHistoryDto createDto, CancellationToken ct = default)
        {
            var history = createDto.ToEntity();
            await _historyRepository.AddAsync(history, ct);
            return history.ToSubscriptionHistoryDto();
        }

        public async Task<SubscriptionHistoryDto> UpdateAsync(Guid id, UpdateSubscriptionHistoryDto updateDto, CancellationToken ct = default)
        {
            var history = await _historyRepository.GetByIdAsync(id, false, ct);
            if (history == null)
                throw new EntityNotFoundException(typeof(SubscriptionHistory), id);

            updateDto.UpdateEntity(history);
            await _historyRepository.UpdateAsync(history, ct);
            return history.ToSubscriptionHistoryDto();
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            await _historyRepository.DeleteAsync(id, ct);
        }

        public async Task<IReadOnlyCollection<SubscriptionHistoryDto>> GetWhereAsync(Expression<Func<SubscriptionHistory, bool>> predicate, bool withIncludes = false, CancellationToken ct = default)
        {
            var histories = await _historyRepository.GetWhereAsync(predicate, withIncludes, ct);
            return histories.Select(h => h.ToSubscriptionHistoryDto()).ToList();
        }

        public async Task<IReadOnlyCollection<SubscriptionHistoryDto>> GetHistoryBySubscriptionAsync(Guid subscriptionId, CancellationToken ct = default)
        {
            var histories = await _historyRepository.GetWhereAsync(
                h => h.SubscriptionId == subscriptionId,
                true, ct);
            return histories.Select(h => h.ToSubscriptionHistoryDto()).ToList();
        }

        public async Task<IReadOnlyCollection<SubscriptionHistoryFullDto>> GetSubscriptionsHistoryByAbonentIdAsync(Guid abonentId, CancellationToken ct = default)
        {
            var subscriptions = await _subscriptionRepository.GetWhereAsync(s => s.Contract != null && s.Contract.AbonentId == abonentId, true, ct);

            if (subscriptions == null || !subscriptions.Any())
                return new List<SubscriptionHistoryFullDto>();

            var subscriptionIds = subscriptions.Select(s => s.Id).ToList();
            var allHistories = new List<SubscriptionHistoryFullDto>();

            foreach (var subId in subscriptionIds)
            {
                var histories = await _historyRepository.GetWhereAsync(h => h.SubscriptionId == subId, true, ct);
                allHistories.AddRange(histories.Select(h => h.ToSubscriptionHistoryFullDto()));
            }

            return allHistories;
        }
    }
}
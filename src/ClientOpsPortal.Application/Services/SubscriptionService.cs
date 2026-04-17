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
    public class SubscriptionService : ISubscriptionService
    {
        private readonly IGenericRepository<Subscription> _subscriptionRepository;
        private readonly IGenericRepository<SubscriptionHistory> _historyRepository;

        public SubscriptionService(
            IGenericRepository<Subscription> subscriptionRepository,
            IGenericRepository<SubscriptionHistory> historyRepository)
        {
            _subscriptionRepository = subscriptionRepository;
            _historyRepository = historyRepository;
        }

        public async Task<SubscriptionDto?> GetByIdAsync(Guid id, bool withIncludes = false, CancellationToken ct = default)
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(id, withIncludes, ct);
            return subscription?.ToSubscriptionDto();
        }

        public async Task<IReadOnlyCollection<SubscriptionDto>> GetAllAsync(bool withIncludes = false, CancellationToken ct = default)
        {
            var subscriptions = await _subscriptionRepository.GetAllAsync(withIncludes, ct);
            return subscriptions.Select(s => s.ToSubscriptionDto()).ToList();
        }

        public async Task<SubscriptionDto> CreateAsync(SubscriptionDto createDto, CancellationToken ct = default)
        {
            var subscription = createDto.ToEntity();
            await _subscriptionRepository.AddAsync(subscription, ct);
            return subscription.ToSubscriptionDto();
        }

        public async Task<SubscriptionDto> UpdateAsync(Guid id, UpdateSubscriptionDto updateDto, CancellationToken ct = default)
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(id, false, ct);
            if (subscription == null)
                throw new EntityNotFoundException(typeof(Subscription), id);

            updateDto.UpdateEntity(subscription);
            await _subscriptionRepository.UpdateAsync(subscription, ct);
            return subscription.ToSubscriptionDto();
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            await _subscriptionRepository.DeleteAsync(id, ct);
        }

        public async Task<IReadOnlyCollection<SubscriptionDto>> GetWhereAsync(Expression<Func<Subscription, bool>> predicate, bool withIncludes = false, CancellationToken ct = default)
        {
            var subscriptions = await _subscriptionRepository.GetWhereAsync(predicate, withIncludes, ct);
            return subscriptions.Select(s => s.ToSubscriptionDto()).ToList();
        }

        public async Task<IReadOnlyCollection<SubscriptionFullDataDto>> GetActiveSubscriptionsByContractAsync(Guid contractId, CancellationToken ct = default)
        {
            var now = DateTimeOffset.UtcNow;
            var subscriptions = await _subscriptionRepository.GetWhereAsync(
                s => s.ContractId == contractId && (s.EndDate == null || s.EndDate > now),
                true, ct);
            return subscriptions.Select(s => s.ToSubscriptionFullDataDto()).ToList();
        }

        public async Task<SubscriptionFullDataDto?> GetFullSubscriptionDataAsync(Guid subscriptionId, CancellationToken ct = default)
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId, true, ct);
            return subscription?.ToSubscriptionFullDataDto();
        }

        public async Task<SubscriptionDto> ChangeTariffPlanAsync(Guid subscriptionId, Guid newTariffPlanId, CancellationToken ct = default)
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId, false, ct);
            if (subscription == null)
                throw new EntityNotFoundException(typeof(Subscription), subscriptionId);

            var history = new SubscriptionHistory
            {
                Id = Guid.NewGuid(),
                SubscriptionId = subscriptionId,
                ActionType = SubscriptionActionType.TariffChange,
                Status = SubscriptionActionStatus.Pending,
                TariffPlanId = newTariffPlanId,
                StartDate = DateTimeOffset.UtcNow
            };
            await _historyRepository.AddAsync(history, ct);

            subscription.TariffPlanId = newTariffPlanId;
            await _subscriptionRepository.UpdateAsync(subscription, ct);

            history.Status = SubscriptionActionStatus.Completed;
            await _historyRepository.UpdateAsync(history, ct);

            return subscription.ToSubscriptionDto();
        }

        public async Task<SubscriptionDto> CancelSubscriptionAsync(Guid subscriptionId, CancellationToken ct = default)
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId, false, ct);
            if (subscription == null)
                throw new EntityNotFoundException(typeof(Subscription), subscriptionId);

            var history = new SubscriptionHistory
            {
                Id = Guid.NewGuid(),
                SubscriptionId = subscriptionId,
                ActionType = SubscriptionActionType.Close,
                Status = SubscriptionActionStatus.Pending,
                TariffPlanId = subscription.TariffPlanId,
                StartDate = DateTimeOffset.UtcNow
            };
            await _historyRepository.AddAsync(history, ct);

            subscription.EndDate = DateTimeOffset.UtcNow;
            await _subscriptionRepository.UpdateAsync(subscription, ct);

            history.Status = SubscriptionActionStatus.Completed;
            await _historyRepository.UpdateAsync(history, ct);

            return subscription.ToSubscriptionDto();
        }

        public async Task<IReadOnlyCollection<SubscriptionFullDataDto>> GetSubscriptionsByAbonentIdAsync(
            Guid abonentId,
            bool onlyActive = true,
            CancellationToken ct = default)
        {
            var now = DateTimeOffset.UtcNow;

            var subscriptions = await _subscriptionRepository.GetWhereAsync(
                s => s.Contract != null && s.Contract.AbonentId == abonentId &&
                     (!onlyActive || (s.EndDate == null || s.EndDate > now)),
                true, ct);

            return subscriptions.Select(s => s.ToSubscriptionFullDataDto()).ToList();
        }
    }
}
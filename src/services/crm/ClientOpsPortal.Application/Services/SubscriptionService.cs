using ClientOpsPortal.Application.DTOs;
using ClientOpsPortal.Application.Interfaces;
using ClientOpsPortal.Application.Mappings;
using ClientOpsPortal.Contracts.Events;
using ClientOpsPortal.Domain.Entities;
using ClientOpsPortal.Domain.Enums;
using ClientOpsPortal.Domain.Exceptions;
using ClientOpsPortal.Domain.Interfaces.Repositories;
using MassTransit;
using System.Linq.Expressions;

namespace ClientOpsPortal.Application.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly IGenericRepository<Subscription> _subscriptionRepository;
        private readonly IGenericRepository<Contract> _contractRepository;
        private readonly ISubscriptionHistoryClient _historyClient;
        private readonly IDirectoryCacheService _cache;
        private readonly IPublishEndpoint _publishEndpoint;

        public SubscriptionService(
            IGenericRepository<Subscription> subscriptionRepository,
            ISubscriptionHistoryClient historyClient,
            IGenericRepository<Contract> contractRepository,
            IDirectoryCacheService cache,
            IPublishEndpoint publishEndpoint)
        {
            _subscriptionRepository = subscriptionRepository;
            _contractRepository = contractRepository;
            _historyClient = historyClient;
            _cache = cache;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<SubscriptionDto?> GetByIdAsync(Guid id, bool withIncludes = false, CancellationToken ct = default)
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(id, withIncludes, ct);
            if (subscription == null) return null;
            await EnrichAsync(subscription, ct);
            return subscription.ToSubscriptionDto();
        }

        public async Task<IReadOnlyCollection<SubscriptionDto>> GetAllAsync(bool withIncludes = false, CancellationToken ct = default)
        {
            var subscriptions = await _subscriptionRepository.GetAllAsync(withIncludes, ct);
            await EnrichAsync(subscriptions, ct);
            return subscriptions.Select(s => s.ToSubscriptionDto()).ToList();
        }

        public async Task<SubscriptionDto> CreateAsync(SubscriptionDto createDto, CancellationToken ct = default)
        {
            var subscription = createDto.ToEntity();

            await _subscriptionRepository.AddAsync(subscription, ct);
            await EnrichAsync(subscription, ct);

            var historyDto = await CreateHistory(
                subscription.Id,
                SubscriptionActionType.Open,
                SubscriptionActionStatus.Pending,
                subscription.TariffPlanId,
                subscription.ContractId,
                ct);

            if (historyDto != null)
            {
                await _historyClient.UpdateHistoryStatusAsync(historyDto.Id, SubscriptionActionStatus.Pending, ct);

                await _historyClient.CreateStepAsync(new CreateSubscriptionHistoryStepDto
                {
                    SubscriptionHistoryId = historyDto.Id,
                    Status = SubscriptionActionStatus.Pending,
                    Message = "Подписка успешно создана"
                }, ct);
            }

            return subscription.ToSubscriptionDto();
        }

        public async Task<SubscriptionDto> UpdateAsync(Guid id, UpdateSubscriptionDto updateDto, CancellationToken ct = default)
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(id, false, ct);
            if (subscription == null)
                throw new EntityNotFoundException(typeof(Subscription), id);
            updateDto.UpdateEntity(subscription);
            await _subscriptionRepository.UpdateAsync(subscription, ct);
            await EnrichAsync(subscription, ct);

            await _publishEndpoint.Publish(new SubscriptionUpdatedEvent(
                subscription.Id, subscription.ContractId, subscription.ServiceId, subscription.TariffPlanId,
                subscription.BeginDate, subscription.EndDate, DateTimeOffset.UtcNow
            ), ct);

            return subscription.ToSubscriptionDto();
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            await _subscriptionRepository.DeleteAsync(id, ct);

            await _publishEndpoint.Publish(new SubscriptionDeletedEvent(id, DateTimeOffset.UtcNow), ct);
        }

        public async Task<IReadOnlyCollection<SubscriptionDto>> GetWhereAsync(Expression<Func<Subscription, bool>> predicate, bool withIncludes = false, CancellationToken ct = default)
        {
            var subscriptions = await _subscriptionRepository.GetWhereAsync(predicate, withIncludes, ct);
            await EnrichAsync(subscriptions, ct);
            return subscriptions.Select(s => s.ToSubscriptionDto()).ToList();
        }

        public async Task<IReadOnlyCollection<SubscriptionFullDataDto>> GetActiveSubscriptionsByContractAsync(Guid contractId, CancellationToken ct = default)
        {
            var now = DateTimeOffset.UtcNow;
            var subscriptions = await _subscriptionRepository.GetWhereAsync(
                s => s.ContractId == contractId && (s.EndDate == null || s.EndDate > now),
                true, ct);
            await EnrichAsync(subscriptions, ct);
            return subscriptions.Select(s => s.ToSubscriptionFullDataDto()).ToList();
        }

        public async Task<SubscriptionFullDataDto?> GetFullSubscriptionDataAsync(Guid subscriptionId, CancellationToken ct = default)
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId, true, ct);
            if (subscription == null) return null;
            await EnrichAsync(subscription, ct);
            return subscription.ToSubscriptionFullDataDto();
        }

        public async Task<SubscriptionDto> ChangeTariffPlanAsync(Guid subscriptionId, Guid newTariffPlanId, CancellationToken ct = default)
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId, false, ct);
            if (subscription == null)
                throw new EntityNotFoundException(typeof(Subscription), subscriptionId);

            var historyDto = await CreateHistory(
                subscription.Id,
                SubscriptionActionType.TariffChange,
                SubscriptionActionStatus.Pending,
                subscription.TariffPlanId,
                subscription.ContractId,
                ct);

            var oldTariffPlanId = subscription.TariffPlanId;
            subscription.TariffPlanId = newTariffPlanId;

            await _subscriptionRepository.UpdateAsync(subscription, ct);
            await EnrichAsync(subscription, ct);

            if (historyDto != null)
            {
                await _historyClient.UpdateHistoryStatusAsync(historyDto.Id, SubscriptionActionStatus.Pending, ct);

                await _historyClient.CreateStepAsync(new CreateSubscriptionHistoryStepDto
                {
                    SubscriptionHistoryId = historyDto.Id,
                    Status = SubscriptionActionStatus.Pending,
                    Message = $"Тарифный план изменен с {oldTariffPlanId} на {newTariffPlanId}"
                }, ct);
            }

            return subscription.ToSubscriptionDto();
        }

        public async Task<SubscriptionDto> CancelSubscriptionAsync(Guid subscriptionId, CancellationToken ct = default)
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId, false, ct);
            if (subscription == null)
                throw new EntityNotFoundException(typeof(Subscription), subscriptionId);

            var historyDto = await CreateHistory(
                subscription.Id,
                SubscriptionActionType.Close,
                SubscriptionActionStatus.Pending,
                subscription.TariffPlanId,
                subscription.ContractId,
                ct);

            subscription.EndDate = DateTimeOffset.UtcNow;

            await _subscriptionRepository.UpdateAsync(subscription, ct);
            await EnrichAsync(subscription, ct);

            if (historyDto != null)
            {
                await _historyClient.UpdateHistoryStatusAsync(historyDto.Id, SubscriptionActionStatus.Pending, ct);

                await _historyClient.CreateStepAsync(new CreateSubscriptionHistoryStepDto
                {
                    SubscriptionHistoryId = historyDto.Id,
                    Status = SubscriptionActionStatus.Pending,
                    Message = "Подписка успешно отключена"
                }, ct);
            }

            return subscription.ToSubscriptionDto();
        }

        public async Task<IReadOnlyCollection<SubscriptionFullDataDto>> GetSubscriptionsByAbonentIdAsync(Guid abonentId, bool onlyActive = true, CancellationToken ct = default)
        {
            var now = DateTimeOffset.UtcNow;
            var subscriptions = await _subscriptionRepository.GetWhereAsync(s => s.Contract != null && s.Contract.AbonentId == abonentId &&
                     (!onlyActive || (s.EndDate == null || s.EndDate > now)),
                true, ct);
            await EnrichAsync(subscriptions, ct);
            return subscriptions.Select(s => s.ToSubscriptionFullDataDto()).ToList();
        }

        private async Task<SubscriptionHistoryDto?> CreateHistory( Guid subscriptionId,
            SubscriptionActionType actionType, SubscriptionActionStatus status, Guid tariffPlanId, Guid contractId, CancellationToken ct)
        {
            try
            {
                var contract = await _contractRepository.GetByIdAsync(contractId, true, ct);
                var abonentId = contract.AbonentId;
                var contractNumber = contract.ContractNumber;

                var subscription = await _subscriptionRepository.GetByIdAsync(subscriptionId, false, ct);
                var serviceId = subscription?.ServiceId ?? Guid.Empty;

                var tariffPlan = await _cache.GetTariffPlanAsync(tariffPlanId, ct);
                var service = await _cache.GetServiceAsync(serviceId, ct);

                var eventDto = new SubscriptionHistoryEventDto
                {
                    SubscriptionId = subscriptionId,
                    ActionType = actionType,
                    Status = status,
                    TariffPlanId = tariffPlanId,
                    TariffPlanName = tariffPlan?.Name ?? string.Empty,
                    ServiceName = service?.Name ?? string.Empty,
                    ContractNumber = contractNumber,
                    AbonentId = abonentId,
                    StartDate = DateTimeOffset.UtcNow,
                    CreatedAt = DateTimeOffset.UtcNow,
                    CreatedBy = "System"
                };

                return await _historyClient.CreateHistoryAsync(eventDto, ct);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private async Task EnrichAsync(Subscription s, CancellationToken ct)
        {
            var svc = await _cache.GetServiceAsync(s.ServiceId, ct);
            if (svc != null)
                s.Service = new Service { Id = svc.Id, Name = svc.Name, Description = svc.Description ?? string.Empty, BeginDate = svc.BeginDate, EndDate = svc.EndDate };
            var tp = await _cache.GetTariffPlanAsync(s.TariffPlanId, ct);
            if (tp != null)
                s.TariffPlan = new TariffPlan { Id = tp.Id, Name = tp.Name, Description = tp.Description ?? string.Empty, Price = tp.Price, ServiceId = tp.ServiceId, BeginDate = tp.BeginDate, EndDate = tp.EndDate };
        }

        private async Task EnrichAsync(IEnumerable<Subscription> list, CancellationToken ct)
        {
            foreach (var s in list)
                await EnrichAsync(s, ct);
        }
    }
}

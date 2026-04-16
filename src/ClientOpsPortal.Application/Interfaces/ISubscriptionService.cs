using ClientOpsPortal.Application.DTOs;
using ClientOpsPortal.Domain.Entities;

namespace ClientOpsPortal.Application.Interfaces
{
    public interface ISubscriptionService : IBaseService<Subscription, SubscriptionDto, SubscriptionDto, UpdateSubscriptionDto>
    {
        Task<IReadOnlyCollection<SubscriptionFullDataDto>> GetActiveSubscriptionsByContractAsync(Guid contractId, CancellationToken ct = default);
        Task<SubscriptionFullDataDto?> GetFullSubscriptionDataAsync(Guid subscriptionId, CancellationToken ct = default);
        Task<SubscriptionDto> ChangeTariffPlanAsync(Guid subscriptionId, Guid newTariffPlanId, CancellationToken ct = default);
        Task<SubscriptionDto> CancelSubscriptionAsync(Guid subscriptionId, CancellationToken ct = default);
        Task<IReadOnlyCollection<SubscriptionFullDataDto>> GetSubscriptionsByAbonentIdAsync(Guid abonentId, bool onlyActive = true, CancellationToken ct = default);
    }
}
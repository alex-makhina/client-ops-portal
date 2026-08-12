using ClientOpsPortal.Application.DTOs;
using ClientOpsPortal.Domain.Entities;
using ClientOpsPortal.Domain.Enums;

namespace ClientOpsPortal.Application.Interfaces
{
    public interface ISubscriptionHistoryService : IBaseService<SubscriptionHistory, SubscriptionHistoryDto, CreateSubscriptionHistoryDto, UpdateSubscriptionHistoryDto>
    {
        Task<IReadOnlyCollection<SubscriptionHistoryDto>> GetHistoryBySubscriptionAsync(Guid subscriptionId, CancellationToken ct = default);
        Task<IReadOnlyCollection<SubscriptionHistoryFullDto>> GetSubscriptionsHistoryByAbonentIdAsync(Guid subscriptionId, CancellationToken ct = default);
    }
}
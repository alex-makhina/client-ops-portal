using ClientOpsPortal.Application.DTOs;
using ClientOpsPortal.Domain.Enums;

namespace ClientOpsPortal.Application.Interfaces
{
    public interface ISubscriptionHistoryClient
    {
        // SubscriptionHistory
        Task<SubscriptionHistoryDto?> GetHistoryByIdAsync(Guid id, CancellationToken ct = default);
        Task<IReadOnlyCollection<SubscriptionHistoryDto>> GetHistoriesBySubscriptionAsync(Guid subscriptionId, CancellationToken ct = default);
        Task<IReadOnlyCollection<SubscriptionHistoryFullDto>> GetHistoriesByAbonentAsync(Guid abonentId, CancellationToken ct = default);
        Task<SubscriptionHistoryDto> CreateHistoryAsync(SubscriptionHistoryEventDto eventDto, CancellationToken ct = default);
        Task UpdateHistoryStatusAsync(Guid historyId, SubscriptionActionStatus status, CancellationToken ct = default);

        // SubscriptionHistoryStep
        Task<SubscriptionHistoryStepDto> CreateStepAsync(CreateSubscriptionHistoryStepDto stepDto, CancellationToken ct = default);
        Task<IReadOnlyCollection<SubscriptionHistoryStepDto>> GetStepsByHistoryAsync(Guid historyId, CancellationToken ct = default);
    }
}
using ClientOpsPortal.Application.DTOs;
using ClientOpsPortal.Domain.Entities;
using ClientOpsPortal.Domain.Enums;

namespace ClientOpsPortal.Application.Interfaces
{
    public interface ISubscriptionHistoryStepService : IBaseService<SubscriptionHistoryStep, SubscriptionHistoryStepDto, CreateSubscriptionHistoryStepDto, UpdateSubscriptionHistoryStepDto>
    {
        Task<IReadOnlyCollection<SubscriptionHistoryStepDto>> GetStepsByHistoryAsync(Guid historyId, CancellationToken ct = default);
    }
}
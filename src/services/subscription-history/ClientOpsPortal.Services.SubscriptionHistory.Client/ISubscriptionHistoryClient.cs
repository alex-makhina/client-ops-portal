using ClientOpsPortal.Services.SubscriptionHistory.Contracts.DTOs;
using ClientOpsPortal.Services.SubscriptionHistory.Contracts.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

using SubscriptionHistoryModel = ClientOpsPortal.Services.SubscriptionHistory.Contracts.Models.SubscriptionHistory;

namespace ClientOpsPortal.Services.SubscriptionHistory.Client
{
    public interface ISubscriptionhistoryClient
    {
        //SubscriptionHistory
        Task<IReadOnlyCollection<SubscriptionHistoryDto>> GetAllSubscriptionHistoryAsync(bool withIncludes = false, CancellationToken ct = default);
        Task<SubscriptionHistoryDto?> GetSubscriptionHistoryByIdAsync(Guid id, bool withIncludes = true, CancellationToken ct = default);
        Task<SubscriptionHistoryDto?> GetSubscriptionHistoryWhereAsync(Expression<Func<SubscriptionHistoryModel, bool>> predicate, Guid id, CancellationToken ct = default);
        Task<IReadOnlyCollection<SubscriptionHistoryDto>> GetHistoryBySubscriptionAsync(Guid subscriptionId, CancellationToken ct = default);
        Task<IReadOnlyCollection<SubscriptionHistoryFullDto>> GetSubscriptionsHistoryByAbonentIdAsync(Guid abonentId, CancellationToken ct = default);
        Task<SubscriptionHistoryDto> CreateSubscriptionHistoryAsync(CreateSubscriptionHistoryDto dto, CancellationToken ct = default);
        Task<SubscriptionHistoryDto> UpdateSubscriptionHistoryAsync(Guid id, UpdateSubscriptionHistoryDto dto, CancellationToken ct = default);
        Task DeleteSubscriptionHistoryAsync(Guid id, CancellationToken ct = default);

        //SubscriptionHistoryStep
        Task<IReadOnlyCollection<SubscriptionHistoryStepDto>> GetAllSubscriptionHistoryStepAsync(bool withIncludes = false, CancellationToken ct = default);
        Task<SubscriptionHistoryStepDto?> GetSubscriptionHistoryStepByIdAsync(Guid id, bool withIncludes = true, CancellationToken ct = default);
        Task<SubscriptionHistoryStepDto?> GetSubscriptionHistoryStepWhereAsync(Expression<Func<SubscriptionHistoryStep, bool>> predicate, Guid id, CancellationToken ct = default);
        Task<IReadOnlyCollection<SubscriptionHistoryStepDto>> GetStepsByHistoryAsync(Guid historyId, CancellationToken ct = default);
        Task<SubscriptionHistoryStepDto> CreateSubscriptionHistoryStepAsync(CreateSubscriptionHistoryStepDto dto, CancellationToken ct = default);
        Task<SubscriptionHistoryStepDto> UpdateSubscriptionHistoryStepAsync(Guid id, UpdateSubscriptionHistoryStepDto dto, CancellationToken ct = default);
        Task DeleteSubscriptionHistoryStepAsync(Guid id, CancellationToken ct = default);
    }
}

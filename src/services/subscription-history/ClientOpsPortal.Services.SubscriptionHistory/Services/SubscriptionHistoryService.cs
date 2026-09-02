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
    public class SubscriptionHistoryService
    {
        private readonly IMongoRepository<SubscriptionHistoryModel> _subscriptionHistoryRepository;
        private readonly IMongoRepository<SubscriptionHistoryStep> _subscriptionHistoryStepRepository;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public SubscriptionHistoryService(
            IMongoRepository<SubscriptionHistoryModel> subscriptionHistoryRepository,
            IMongoRepository<SubscriptionHistoryStep> subscriptionHistoryStepRepository)
        {
            _subscriptionHistoryRepository = subscriptionHistoryRepository;
            _subscriptionHistoryStepRepository = subscriptionHistoryStepRepository;
        }

        public async Task<SubscriptionHistoryDto?> GetSubscriptionHistoryByIdAsync(Guid id, CancellationToken ct = default)
        {
            var history = await _subscriptionHistoryRepository.GetByIdAsync(id, ct);
            return history?.ToSubscriptionHistoryDto();
        }

        public async Task<IReadOnlyCollection<SubscriptionHistoryDto>> GetAllSubscriptionHistoryAsync(CancellationToken ct = default)
        {
            var histories = await _subscriptionHistoryRepository.GetAllAsync(ct);
            return histories.Select(h => h.ToSubscriptionHistoryDto()).ToList();
        }

        public async Task<SubscriptionHistoryDto> CreateSubscriptionHistoryAsync(CreateSubscriptionHistoryDto createDto, CancellationToken ct = default)
        {
            var history = createDto.ToEntity();
            await _subscriptionHistoryRepository.AddAsync(history, ct);
            return history.ToSubscriptionHistoryDto();
        }

        public async Task<SubscriptionHistoryDto> UpdateSubscriptionHistoryAsync(Guid id, UpdateSubscriptionHistoryDto updateDto, CancellationToken ct = default)
        {
            var history = await _subscriptionHistoryRepository.GetByIdAsync(id, ct);
            if (history == null)
                throw new EntityNotFoundException(typeof(SubscriptionHistoryModel), id);
            updateDto.UpdateEntity(history);
            await _subscriptionHistoryRepository.UpdateAsync(history, ct);
            return history.ToSubscriptionHistoryDto();
        }

        public async Task DeleteSubscriptionHistoryAsync(Guid id, CancellationToken ct = default)
        {
            await _subscriptionHistoryRepository.DeleteAsync(id, ct);
        }

        public async Task<IReadOnlyCollection<SubscriptionHistoryDto>> GetSubscriptionHistoryWhereAsync(Expression<Func<SubscriptionHistoryModel, bool>> predicate, CancellationToken ct = default)
        {
            var histories = await _subscriptionHistoryRepository.GetWhereAsync(predicate, ct);
            return histories.Select(h => h.ToSubscriptionHistoryDto()).ToList();
        }

        public async Task<IReadOnlyCollection<SubscriptionHistoryDto>> GetHistoryBySubscriptionAsync(Guid subscriptionId, CancellationToken ct = default)
        {
            var histories = await _subscriptionHistoryRepository.GetWhereAsync(
                h => h.SubscriptionId == subscriptionId, ct);
            return histories.OrderByDescending(h => h.CreatedAt).Select(h => h.ToSubscriptionHistoryDto()).ToList();
        }

        public async Task<IReadOnlyCollection<SubscriptionHistoryFullDto>> GetSubscriptionsHistoryByAbonentIdAsync(Guid abonentId, CancellationToken ct = default)
        {
            var histories = await _subscriptionHistoryRepository.GetWhereAsync(s => s.AbonentId == abonentId, ct); var result = new List<SubscriptionHistoryFullDto>();

            foreach (var history in histories)
            {
                var steps = await _subscriptionHistoryStepRepository.GetWhereAsync(
                    s => s.SubscriptionHistoryId == history.Id, ct);

                var dto = history.ToSubscriptionHistoryFullDto();
                dto.Steps = steps?.Select(s => new SubscriptionHistoryStep
                {
                    Id = s.Id,
                    SubscriptionHistoryId = s.SubscriptionHistoryId,
                    Status = s.Status,
                    Message = s.Message,
                    CreatedAt = s.CreatedAt,
                    CreatedBy = s.CreatedBy
                }).ToList() ?? new List<SubscriptionHistoryStep>();

                result.Add(dto);
            }

            return result.OrderByDescending(h => h.CreatedAt).ToList();
        }
    }
}

using ClientOpsPortal.Services.SubscriptionHistory.Contracts.DTOs;
using ClientOpsPortal.Services.SubscriptionHistory.Contracts.Models;

using SubscriptionHistoryModel = ClientOpsPortal.Services.SubscriptionHistory.Contracts.Models.SubscriptionHistory;

namespace ClientOpsPortal.Services.SubscriptionHistory.Mappings
{
    public static class SubscriptionHistoryMapper
    {
        public static SubscriptionHistoryDto ToSubscriptionHistoryDto(this SubscriptionHistoryModel history)
        {
            return new SubscriptionHistoryDto
            {
                Id = history.Id,
                SubscriptionId = history.SubscriptionId,
                ActionType = history.ActionType,
                Status = history.Status,
                TariffPlanId = history.TariffPlanId,
                StartDate = history.StartDate,
                Steps = history.Steps.ToList(),
                CreatedAt = history.CreatedAt,
                CreatedBy = history.CreatedBy,
                UpdatedAt = history.UpdatedAt,
                UpdatedBy = history.UpdatedBy
            };
        }

        public static SubscriptionHistoryModel ToEntity(this CreateSubscriptionHistoryDto createDto)
        {
            var now = DateTimeOffset.UtcNow;

            return new SubscriptionHistoryModel
            {
                Id = Guid.NewGuid(),
                SubscriptionId = createDto.SubscriptionId,
                ActionType = createDto.ActionType,
                Status = createDto.Status,
                TariffPlanId = createDto.TariffPlanId,
                TariffPlanName = createDto.TariffPlanName ?? string.Empty,
                ServiceName = createDto.ServiceName ?? string.Empty,
                ContractNumber = createDto.ContractNumber ?? string.Empty,
                AbonentId = createDto.AbonentId,
                StartDate = createDto.StartDate != default ? createDto.StartDate : now,
                Steps = createDto.Steps ?? new(),
                CreatedAt = now,
            };
        }

        public static void UpdateEntity(this UpdateSubscriptionHistoryDto updateDto, SubscriptionHistoryModel entity)
        {
            entity.Status = updateDto.Status;
        }

        public static SubscriptionHistoryFullDto ToSubscriptionHistoryFullDto(this SubscriptionHistoryModel history)
        {
            return new SubscriptionHistoryFullDto
            {
                Id = history.Id,
                SubscriptionId = history.SubscriptionId,
                ActionType = history.ActionType,
                Status = history.Status,
                ServiceName = history.ServiceName,
                TariffPlanId = history.TariffPlanId,
                TariffPlanName = history.TariffPlanName,
                ContractNumber = history.ContractNumber,
                AbonentId = history.AbonentId,
                StartDate = history.StartDate,
                CreatedAt = history.CreatedAt,
                CreatedBy = history.CreatedBy,
                UpdatedAt = history.UpdatedAt,
                UpdatedBy = history.UpdatedBy,
                Steps = history.Steps?.Select(s => new SubscriptionHistoryStep
                {
                    Id = s.Id,
                    SubscriptionHistoryId = s.SubscriptionHistoryId,
                    Status = s.Status,
                    Message = s.Message,
                    CreatedAt = s.CreatedAt,
                    CreatedBy = s.CreatedBy
                }).ToList() ?? new List<SubscriptionHistoryStep>()
            };
        }
    }
}

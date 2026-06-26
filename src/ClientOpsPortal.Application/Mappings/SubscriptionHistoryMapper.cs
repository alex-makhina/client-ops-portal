using ClientOpsPortal.Application.DTOs;
using ClientOpsPortal.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClientOpsPortal.Application.Mappings
{
    public static class SubscriptionHistoryMapper
    {
        public static SubscriptionHistoryDto ToSubscriptionHistoryDto(this SubscriptionHistory history)
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

        public static SubscriptionHistory ToEntity(this CreateSubscriptionHistoryDto createDto)
        {
            return new SubscriptionHistory
            {
                Id = Guid.NewGuid(),
                SubscriptionId = createDto.SubscriptionId,
                ActionType = createDto.ActionType,
                Status = createDto.Status,
                TariffPlanId = createDto.TariffPlanId,
                StartDate = createDto.StartDate,
                Steps = createDto.Steps ?? new()
            };
        }

        public static void UpdateEntity(this UpdateSubscriptionHistoryDto updateDto, SubscriptionHistory entity)
        {
            entity.Status = updateDto.Status;
        }

        public static SubscriptionHistoryFullDto ToSubscriptionHistoryFullDto(this SubscriptionHistory history)
        {
            return new SubscriptionHistoryFullDto
            {
                Id = history.Id,
                SubscriptionId = history.SubscriptionId,
                ActionType = history.ActionType,
                Status = history.Status,
                StartDate = history.StartDate,
                CreatedAt = history.CreatedAt,
                ServiceName = history.Subscription.Service?.Name,
                TariffPlanName = history.TariffPlan?.Name,
                ContractNumber = history.Subscription.Contract?.ContractNumber,
                Steps = history.Steps?.Select(s => new SubscriptionHistoryStep
                {
                    Id = s.Id,
                    SubscriptionHistoryId = s.SubscriptionHistoryId,
                    Status = s.Status,
                    Message = s.Message,
                    CreatedAt = s.CreatedAt
                }).ToList() ?? new List<SubscriptionHistoryStep>()
            };
        }
    }
}

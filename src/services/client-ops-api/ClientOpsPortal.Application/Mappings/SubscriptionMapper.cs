using ClientOpsPortal.Application.DTOs;
using ClientOpsPortal.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClientOpsPortal.Application.Mappings
{
    public static class SubscriptionMapper
    {
        public static SubscriptionDto ToSubscriptionDto(this Subscription subscription)
        {
            return new SubscriptionDto
            {
                Id = subscription.Id,
                ContractId = subscription.ContractId,
                ServiceId = subscription.ServiceId,
                TariffPlanId = subscription.TariffPlanId,
                BeginDate = subscription.BeginDate,
                EndDate = subscription.EndDate,
                CreatedAt = subscription.CreatedAt,
                CreatedBy = subscription.CreatedBy,
                UpdatedAt = subscription.UpdatedAt,
                UpdatedBy = subscription.UpdatedBy
            };
        }

        public static SubscriptionFullDataDto ToSubscriptionFullDataDto(this Subscription subscription)
        {
            return new SubscriptionFullDataDto
            {
                Id = subscription.Id,
                ContractId = subscription.ContractId,
                ContractNumber = subscription.Contract.ContractNumber,
                ServiceId = subscription.ServiceId,
                ServiceName = subscription.Service.Name,
                TariffPlanId = subscription.TariffPlanId,
                TariffPlanName = subscription.TariffPlan.Name,
                BeginDate = subscription.BeginDate,
                EndDate = subscription.EndDate
            };
        }

        public static Subscription ToEntity(this SubscriptionDto createDto)
        {
            return new Subscription
            {
                Id = Guid.NewGuid(),
                ContractId = createDto.ContractId,
                ServiceId = createDto.ServiceId,
                TariffPlanId = createDto.TariffPlanId,
                BeginDate = createDto.BeginDate.ToUniversalTime(),
                EndDate = createDto.EndDate?.ToUniversalTime()
            };
        }

        public static void UpdateEntity(this UpdateSubscriptionDto updateDto, Subscription entity)
        {
            if (updateDto.TariffPlanId.HasValue)
                entity.TariffPlanId = updateDto.TariffPlanId.Value;
            if (updateDto.EndDate.HasValue)
                entity.EndDate = updateDto.EndDate;
        }
    }
}

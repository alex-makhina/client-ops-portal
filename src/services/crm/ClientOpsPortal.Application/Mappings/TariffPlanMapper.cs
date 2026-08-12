using ClientOpsPortal.Application.DTOs;
using ClientOpsPortal.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClientOpsPortal.Application.Mappings
{
    public static class TariffPlanMapper
    {
        public static TariffPlanDto ToTariffPlanDto(this TariffPlan tariff)
        {
            return new TariffPlanDto
            {
                Id = tariff.Id,
                Name = tariff.Name,
                Description = tariff.Description,
                Price = tariff.Price,
                ServiceId = tariff.ServiceId,
                BeginDate = tariff.BeginDate,
                EndDate = tariff.EndDate
            };
        }

        public static TariffPlanShortDataDto ToTariffPlanShortDataDto(this TariffPlan tariff)
        {
            return new TariffPlanShortDataDto
            {
                Id = tariff.Id,
                Name = tariff.Name,             
                Price = tariff.Price
            };
        }

        public static TariffPlan ToEntity(this CreateTariffPlanDto createDto)
        {
            return new TariffPlan
            {
                Id = Guid.NewGuid(),
                Name = createDto.Name,
                Description = createDto.Description,
                Price = createDto.Price,
                ServiceId = createDto.ServiceId,
                BeginDate = createDto.BeginDate,
                EndDate = createDto.EndDate
            };
        }

        public static TariffPlan ToEntityFromUpdateItem(this UpdateTariffPlanFromServiceDto updateItemDto, Guid serviceId)
        {
            return new TariffPlan
            {
                Id = updateItemDto.Id == Guid.Empty ? Guid.NewGuid() : updateItemDto.Id,
                Name = updateItemDto.Name,
                Description = updateItemDto.Description,
                Price = updateItemDto.Price,
                ServiceId = serviceId,
                BeginDate = updateItemDto.BeginDate,
                EndDate = updateItemDto.EndDate
            };
        }

        public static void UpdateEntityPartial(this UpdateTariffPlanDto updateDto, TariffPlan entity)
        {
            if (!string.IsNullOrWhiteSpace(updateDto.Name))
                entity.Name = updateDto.Name;

            if (!string.IsNullOrWhiteSpace(updateDto.Description))
                entity.Description = updateDto.Description;

            if (updateDto.Price.HasValue && updateDto.Price.Value > 0)
                entity.Price = updateDto.Price.Value;

            if (updateDto.BeginDate.HasValue && updateDto.BeginDate.Value != default)
                entity.BeginDate = updateDto.BeginDate.Value;

            if (updateDto.EndDate.HasValue)
                entity.EndDate = updateDto.EndDate;
        }

        public static void UpdateEntityFull(this UpdateTariffPlanFromServiceDto updateDto, TariffPlan entity)
        {
            entity.Name = updateDto.Name;
            entity.Description = updateDto.Description;
            entity.Price = updateDto.Price;
            entity.BeginDate = updateDto.BeginDate;
            entity.EndDate = updateDto.EndDate;
        }
    }
}

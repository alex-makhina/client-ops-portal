using ClientOpsPortal.Application.DTOs;
using ClientOpsPortal.Domain.Entities;
using ClientOpsPortal.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClientOpsPortal.Application.Mappings
{
    public static class ServiceMapper
    {
        public static ServiceDto ToServiceDto(this Service service)
        {
            return new ServiceDto
            {
                Id = service.Id,
                Name = service.Name,
                Description = service.Description,
                BeginDate = service.BeginDate,
                EndDate = service.EndDate,
                CreatedAt = service.CreatedAt,
                CreatedBy = service.CreatedBy, 
                UpdatedAt = service.UpdatedAt,
                UpdatedBy = service.UpdatedBy
            };
        }

        public static ServiceShortDataDto ToServiceShortDataDto(this Service service)
        {
            return new ServiceShortDataDto
            {
                Id = service.Id,
                Name = service.Name,
                Description = service.Description,
                BeginDate = service.BeginDate,
                EndDate = service.EndDate
            };
        }

        public static ServiceFullDataDto ToServiceFullDataDto(this Service service)
        {
            return new ServiceFullDataDto
            {
                Id = service.Id,
                Name = service.Name,
                Description = service.Description,
                BeginDate = service.BeginDate,
                EndDate = service.EndDate,
                TariffPlans = service.TariffPlans?.Select(tp => tp.ToTariffPlanDto()).ToList() ?? new List<TariffPlanDto>()
            };
        }

        public static Service ToEntity(this CreateServiceDto createDto)
        {
            var serviceId = Guid.NewGuid();
            var now = DateTimeOffset.UtcNow;

            var service = new Service
            {
                Id = serviceId,
                Name = createDto.Name,
                Description = createDto.Description,
                BeginDate = createDto.BeginDate.ToUniversalTime(),
                EndDate = createDto.EndDate?.ToUniversalTime(),
                TariffPlans = new List<TariffPlan>()
            };

            if (createDto.TariffPlans != null && createDto.TariffPlans.Any())
            {
                foreach (var tariffDto in createDto.TariffPlans)
                {
                    service.TariffPlans.Add(new TariffPlan
                    {
                        Id = Guid.NewGuid(),
                        Name = tariffDto.Name,
                        Description = tariffDto.Description,
                        Price = tariffDto.Price,
                        ServiceId = serviceId, 
                        BeginDate = tariffDto.BeginDate.ToUniversalTime(),
                        EndDate = tariffDto.EndDate?.ToUniversalTime()
                    });
                }
            }

            return service;
        }     
    }
}

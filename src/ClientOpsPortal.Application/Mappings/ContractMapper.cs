using ClientOpsPortal.Application.DTOs;
using ClientOpsPortal.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClientOpsPortal.Application.Mappings
{
    public static class ContractMapper
    {
        public static ContractDataDto ToContractDto(this Contract contract)
        {
            return new ContractDataDto
            {
                Id = contract.Id,
                ContractNumber = contract.ContractNumber,
                AbonentId = contract.AbonentId,
                BeginDate = contract.BeginDate,
                EndDate = contract.EndDate,
                CreatedAt = contract.CreatedAt,
                CreatedBy = contract.CreatedBy, 
                UpdatedAt = contract.UpdatedAt,
                UpdatedBy = contract.UpdatedBy
            };
        }

        public static ContractShortDataDto ToShortDto(this Contract contract)
        {
            return new ContractShortDataDto
            {
                ContractNumber = contract.ContractNumber,
                AbonentId = contract.AbonentId,
                BeginDate = contract.BeginDate
            };
        }

        public static Contract ToEntity(this ContractDataDto createDto)
        {
            return new Contract
            {
                Id = Guid.NewGuid(),
                ContractNumber = createDto.ContractNumber,
                AbonentId = createDto.AbonentId,
                BeginDate = createDto.BeginDate,
                EndDate = createDto.EndDate
            };
        }

        public static void UpdateEntity(this UpdateContractDto updateDto, Contract entity)
        {
            entity.EndDate = updateDto.EndDate;
        }
    }
}

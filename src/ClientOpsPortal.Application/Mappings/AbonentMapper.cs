using ClientOpsPortal.Application.DTOs;
using ClientOpsPortal.Domain.Entities;

namespace ClientOpsPortal.Application.Mappings
{
    public static class AbonentMapper
    {
        public static AbonentDto ToAbonentDto(this Abonent abonent)
        {
            return new AbonentDto
            {
                Id = abonent.Id,
                UserId = abonent.UserId,
                IdentificationNumber = abonent.IdentificationNumber,
                FirstName = abonent.FirstName,
                LastName = abonent.LastName,
                MiddleName = abonent.MiddleName,
                AccountNumber = abonent.AccountNumber,
                CreatedAt = abonent.CreatedAt,
                CreatedBy = abonent.CreatedBy,
                UpdatedAt = abonent.UpdatedAt,
                UpdatedBy = abonent.UpdatedBy
            };
        }

        public static AbonentShortDataDto ToAbonentShortDataDto(this Abonent abonent)
        {
            return new AbonentShortDataDto
            {
                FullName = abonent.FullName,
                AccountNumber = abonent.AccountNumber
            };
        }

        public static Abonent ToEntity(this CreateAbonentDto createDto)
        {
            return new Abonent
            {
                Id = Guid.NewGuid(),
                IdentificationNumber = createDto.IdentificationNumber,
                FirstName = createDto.FirstName,
                LastName = createDto.LastName,
                MiddleName = createDto.MiddleName,
                AccountNumber = createDto.AccountNumber
            };
        }
  
        public static void UpdateEntity(this UpdateAbonentDto updateDto, Abonent entity)
        {
            entity.IdentificationNumber = updateDto.IdentificationNumber;
            entity.FirstName = updateDto.FirstName;
            entity.LastName = updateDto.LastName;
            entity.MiddleName = updateDto.MiddleName;
            entity.AccountNumber = updateDto.AccountNumber;
        }
    }
}

using ClientOpsPortal.Application.DTOs;
using ClientOpsPortal.Domain.Entities;

namespace ClientOpsPortal.Application.Interfaces
{
    public interface IAbonentService : IBaseService<Abonent, AbonentDto, CreateAbonentDto, UpdateAbonentDto>
    {
        Task<IReadOnlyCollection<AbonentShortDataDto>> SearchByFullNameAsync(string searchTerm, CancellationToken ct = default);
        Task<AbonentDto?> GetByContractNumberAsync(string contractNumber, CancellationToken ct = default);
    }
}
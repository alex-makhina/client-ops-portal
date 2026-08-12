using ClientOpsPortal.Application.DTOs;
using ClientOpsPortal.Domain.Entities;

namespace ClientOpsPortal.Application.Interfaces
{
    public interface IContractService : IBaseService<Contract, ContractDataDto, ContractDataDto, UpdateContractDto>
    {
        Task<IReadOnlyCollection<ContractShortDataDto>> GetShortContractsByAbonentAsync(Guid abonentId, CancellationToken ct = default);
    }
}
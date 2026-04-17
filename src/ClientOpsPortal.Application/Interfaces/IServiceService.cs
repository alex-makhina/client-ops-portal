using ClientOpsPortal.Application.DTOs;
using ClientOpsPortal.Domain.Entities;

namespace ClientOpsPortal.Application.Interfaces
{
    public interface IServiceService : IBaseService<Service, ServiceDto, CreateServiceDto, UpdateServiceDto>
    {
        Task<ServiceFullDataDto?> GetFullServiceDataAsync(Guid serviceId, CancellationToken ct = default);
        Task<IReadOnlyCollection<ServiceShortDataDto>> GetActiveServicesAsync(CancellationToken ct = default);
    }
}
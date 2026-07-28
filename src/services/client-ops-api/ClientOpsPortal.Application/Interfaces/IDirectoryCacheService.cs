using ClientOpsPortal.Services.Directory.Contracts.DTOs;

namespace ClientOpsPortal.Application.Interfaces
{
    public interface IDirectoryCacheService
    {
        Task<ServiceDto?> GetServiceAsync(Guid id, CancellationToken ct = default);
        Task<TariffPlanDto?> GetTariffPlanAsync(Guid id, CancellationToken ct = default);
    }
}
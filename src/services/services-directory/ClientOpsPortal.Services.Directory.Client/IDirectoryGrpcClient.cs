using ClientOpsPortal.Services.Directory.Contracts.DTOs;

namespace ClientOpsPortal.Services.Directory.Client
{
    public interface IDirectoryGrpcClient
    {
        Task<IReadOnlyCollection<ServiceDto>> GetAllServicesAsync(bool withIncludes = false, CancellationToken ct = default);
        Task<IReadOnlyCollection<TariffPlanDto>> GetAllTariffPlansAsync(bool withIncludes = false, CancellationToken ct = default);
    }
}

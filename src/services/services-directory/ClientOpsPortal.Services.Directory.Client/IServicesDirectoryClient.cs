using ClientOpsPortal.Services.Directory.Contracts.DTOs;

namespace ClientOpsPortal.Services.Directory.Client
{
    public interface IServicesDirectoryClient
    {
        // Services
        Task<IReadOnlyCollection<ServiceDto>> GetAllServicesAsync(bool withIncludes = false, CancellationToken ct = default);
        Task<ServiceDto?> GetServiceByIdAsync(Guid id, bool withIncludes = true, CancellationToken ct = default);
        Task<ServiceFullDataDto?> GetFullServiceDataAsync(Guid id, CancellationToken ct = default);
        Task<IReadOnlyCollection<ServiceShortDataDto>> GetActiveServicesAsync(CancellationToken ct = default);
        Task<ServiceDto> CreateServiceAsync(CreateServiceDto dto, CancellationToken ct = default);
        Task<ServiceDto> UpdateServiceAsync(Guid id, UpdateServiceDto dto, CancellationToken ct = default);
        Task DeleteServiceAsync(Guid id, CancellationToken ct = default);

        // TariffPlans
        Task<IReadOnlyCollection<TariffPlanDto>> GetAllTariffPlansAsync(bool withIncludes = false, CancellationToken ct = default);
        Task<TariffPlanDto?> GetTariffPlanByIdAsync(Guid id, bool withIncludes = true, CancellationToken ct = default);
        Task<IReadOnlyCollection<TariffPlanDto>> GetTariffPlansByServiceAsync(Guid serviceId, CancellationToken ct = default);
        Task<IReadOnlyCollection<TariffPlanShortDataDto>> GetActiveTariffPlansByServiceAsync(Guid serviceId, CancellationToken ct = default);
        Task<TariffPlanDto> CreateTariffPlanAsync(CreateTariffPlanDto dto, CancellationToken ct = default);
        Task<TariffPlanDto> UpdateTariffPlanAsync(Guid id, UpdateTariffPlanDto dto, CancellationToken ct = default);
        Task DeleteTariffPlanAsync(Guid id, CancellationToken ct = default);
    }
}

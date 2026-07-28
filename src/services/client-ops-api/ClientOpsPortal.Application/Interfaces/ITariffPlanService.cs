using ClientOpsPortal.Application.DTOs;
using ClientOpsPortal.Domain.Entities;

namespace ClientOpsPortal.Application.Interfaces
{
    public interface ITariffPlanService : IBaseService<TariffPlan, TariffPlanDto, CreateTariffPlanDto, UpdateTariffPlanDto>
    {
        Task<IReadOnlyCollection<TariffPlanShortDataDto>> GetActiveTariffPlansByServiceAsync(Guid serviceId, CancellationToken ct = default);
        Task<IReadOnlyCollection<TariffPlanDto>> GetTariffPlansByServiceAsync(Guid serviceId, CancellationToken ct = default);
    }
}
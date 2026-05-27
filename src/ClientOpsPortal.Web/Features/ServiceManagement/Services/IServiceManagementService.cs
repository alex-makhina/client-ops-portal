using ClientOpsPortal.Web.Features.ServiceManagement.Models;

public interface IServiceManagementService
{
    Task<List<ServiceListItem>> GetAllServicesAsync();
    Task<ServiceFullItem?> GetFullServiceByIdAsync(Guid serviceId);
    Task<bool> CreateServiceAsync(CreateServiceRequest request);
    Task<bool> UpdateServiceAsync(Guid serviceId, UpdateServiceRequest request);
    Task<bool> DeleteServiceAsync(Guid serviceId);
}
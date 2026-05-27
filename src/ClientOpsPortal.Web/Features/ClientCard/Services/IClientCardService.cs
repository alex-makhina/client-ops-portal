using ClientOpsPortal.Web.Features.ClientCard.Models;

namespace ClientOpsPortal.Web.Features.ClientCard.Services
{
    public interface IClientCardService
    {
        Task<AbonentDetail?> GetAbonentAsync(Guid abonentId);
        Task<bool> UpdateAbonentAsync(Guid abonentId, UpdateAbonentModel model);
        Task<IReadOnlyCollection<ContractItem>> GetContractsAsync(Guid abonentId);
        Task<bool> CreateContractAsync(CreateContractModel model);
        Task<IReadOnlyCollection<SubscriptionItem>> GetSubscriptionsByContractAsync(Guid contractId);
        Task<bool> ConnectServiceAsync(ConnectServiceModel model);
        Task<bool> CancelSubscriptionAsync(Guid subscriptionId);
    }
}

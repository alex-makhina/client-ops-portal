using ClientOpsPortal.Application.Interfaces;
using ClientOpsPortal.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace ClientOpsPortal.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public abstract class BaseController : ControllerBase
    {
        protected readonly ICurrentUserService CurrentUserService;

        protected BaseController(ICurrentUserService currentUserService)
        {
            CurrentUserService = currentUserService;
        }

        protected bool IsCurrentUserAbonentOwner(Guid abonentUserId)
        {
            if (!User.IsInRole("Abonent"))
                return true;

            return CurrentUserService.UserId == abonentUserId;
        }

        protected async Task<bool> IsAbonentOwnerAsync(
            Guid abonentId,
            IAbonentService abonentService,
            CancellationToken ct = default)
        {
            if (!User.IsInRole("Abonent"))
                return true;

            var abonent = await abonentService.GetByIdAsync(abonentId, false, ct);
            return abonent != null && CurrentUserService.UserId == abonent.UserId;
        }

        protected async Task<bool> IsContractOwnerAsync(
            Guid contractId,
            IContractService contractService,
            IAbonentService abonentService,
            CancellationToken ct = default)
        {
            if (!User.IsInRole("Abonent"))
                return true;

            var contract = await contractService.GetByIdAsync(contractId, false, ct);
            if (contract == null)
                return false;

            var abonent = await abonentService.GetByIdAsync(contract.AbonentId, false, ct);
            return abonent != null && CurrentUserService.UserId == abonent.UserId;
        }

        protected async Task<bool> IsSubscriptionOwnerAsync(
            Guid subscriptionId,
            ISubscriptionService subscriptionService,
            IContractService contractService,
            IAbonentService abonentService,
            CancellationToken ct = default)
        {
            if (!User.IsInRole("Abonent"))
                return true;

            var subscription = await subscriptionService.GetFullSubscriptionDataAsync(subscriptionId, ct);
            if (subscription == null)
                return false;

            var contract = await contractService.GetByIdAsync(subscription.ContractId, false, ct);
            if (contract == null)
                return false;

            var abonent = await abonentService.GetByIdAsync(contract.AbonentId, false, ct);
            return abonent != null && CurrentUserService.UserId == abonent.UserId;
        }
    }
}

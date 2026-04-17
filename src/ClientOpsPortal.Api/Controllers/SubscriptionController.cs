using ClientOpsPortal.Application.DTOs;
using ClientOpsPortal.Application.Interfaces;
using ClientOpsPortal.Domain.Entities;
using ClientOpsPortal.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace ClientOpsPortal.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubscriptionsController : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;

        public SubscriptionsController(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool withIncludes = false, CancellationToken ct = default)
        {
            var subscriptions = await _subscriptionService.GetAllAsync(withIncludes, ct);
            return Ok(subscriptions);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, [FromQuery] bool withIncludes = true, CancellationToken ct = default)
        {
            var subscription = await _subscriptionService.GetByIdAsync(id, withIncludes, ct);
            if (subscription == null)
                return NotFound($"Подписка с ID {id} не найдена");
            return Ok(subscription);
        }

        [HttpGet("full/{id}")]
        public async Task<IActionResult> GetFullSubscriptionData(Guid id, CancellationToken ct = default)
        {
            var subscription = await _subscriptionService.GetFullSubscriptionDataAsync(id, ct);
            if (subscription == null)
                return NotFound($"Подписка с ID {id} не найдена");
            return Ok(subscription);
        }

        [HttpGet("by-contract/{contractId}")]
        public async Task<IActionResult> GetByContract(Guid contractId, CancellationToken ct = default)
        {
            var subscriptions = await _subscriptionService.GetActiveSubscriptionsByContractAsync(contractId, ct);
            return Ok(subscriptions);
        }

        [HttpGet("by-abonent/{abonentId}")]
        public async Task<IActionResult> GetByAbonentId(
            Guid abonentId,
            [FromQuery] bool onlyActive = true,
            CancellationToken ct = default)
        {
            var subscriptions = await _subscriptionService.GetSubscriptionsByAbonentIdAsync(abonentId, onlyActive, ct);

            if (!subscriptions.Any())
            {
                return NotFound($"Подписки для абонента с ID {abonentId} не найдены");
            }

            return Ok(subscriptions);
        }

        [HttpPost]
        public async Task<IActionResult> Create(SubscriptionDto createDto, CancellationToken ct = default)
        {
            try
            {
                var subscription = await _subscriptionService.CreateAsync(createDto, ct);
                return CreatedAtAction(nameof(GetById), new { id = subscription.Id }, subscription);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateSubscriptionDto updateDto, CancellationToken ct = default)
        {
            try
            {
                var subscription = await _subscriptionService.UpdateAsync(id, updateDto, ct);
                return Ok(subscription);
            }
            catch (EntityNotFoundException)
            {
                return NotFound($"Подписка с ID {id} не найдена");
            }
        }

        [HttpPatch("{id}/change-tariff")]
        public async Task<IActionResult> ChangeTariffPlan(Guid id, [FromQuery] Guid newTariffPlanId, CancellationToken ct = default)
        {
            try
            {
                var subscription = await _subscriptionService.ChangeTariffPlanAsync(id, newTariffPlanId, ct);
                return Ok(subscription);
            }
            catch (EntityNotFoundException)
            {
                return NotFound($"Подписка с ID {id} не найдена");
            }
        }

        [HttpPatch("{id}/cancel")]
        public async Task<IActionResult> CancelSubscription(Guid id, CancellationToken ct = default)
        {
            try
            {
                var subscription = await _subscriptionService.CancelSubscriptionAsync(id, ct);
                return Ok(subscription);
            }
            catch (EntityNotFoundException)
            {
                return NotFound($"Подписка с ID {id} не найдена");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
        {
            try
            {
                await _subscriptionService.DeleteAsync(id, ct);
                return NoContent();
            }
            catch (EntityNotFoundException)
            {
                return NotFound($"Подписка с ID {id} не найдена");
            }
        }
    }
}
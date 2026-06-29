using ClientOpsPortal.Application.DTOs;
using ClientOpsPortal.Application.Interfaces;
using ClientOpsPortal.Domain.Entities;
using ClientOpsPortal.Domain.Enums;
using ClientOpsPortal.Domain.Exceptions;
using ClientOpsPortal.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClientOpsPortal.Api.Controllers
{
    [Authorize]
    public class SubscriptionHistoriesController : BaseController
    {
        private readonly ISubscriptionHistoryService _historyService;

        public SubscriptionHistoriesController(
            ISubscriptionHistoryService historyService,
            ICurrentUserService currentUserService)
            : base(currentUserService)
        {
            _historyService = historyService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool withIncludes = false, CancellationToken ct = default)
        {
            var histories = await _historyService.GetAllAsync(withIncludes, ct);
            return Ok(histories);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, [FromQuery] bool withIncludes = true, CancellationToken ct = default)
        {
            var history = await _historyService.GetByIdAsync(id, withIncludes, ct);
            if (history == null)
                return NotFound($"История с ID {id} не найдена");
            return Ok(history);
        }

        [HttpGet("by-subscription/{subscriptionId}")]
        public async Task<IActionResult> GetBySubscription(Guid subscriptionId, CancellationToken ct = default)
        {
            var histories = await _historyService.GetWhereAsync(h => h.SubscriptionId == subscriptionId, true, ct);
            return Ok(histories);
        }

        [HttpGet("by-abonent/{abonentId}")]
        public async Task<IActionResult> GetByAbonent(Guid abonentId, CancellationToken ct = default)
        {
            var histories = await _historyService.GetSubscriptionsHistoryByAbonentIdAsync(abonentId, ct);
            return Ok(histories);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateSubscriptionHistoryDto createDto, CancellationToken ct = default)
        {
            try
            {
                var history = await _historyService.CreateAsync(createDto, ct);
                return CreatedAtAction(nameof(GetById), new { id = history.Id }, history);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateSubscriptionHistoryDto updateDto, CancellationToken ct = default)
        {
            try
            {
                var history = await _historyService.UpdateAsync(id, updateDto, ct);
                return Ok(history);
            }
            catch (EntityNotFoundException)
            {
                return NotFound($"История с ID {id} не найдена");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
        {
            try
            {
                await _historyService.DeleteAsync(id, ct);
                return NoContent();
            }
            catch (EntityNotFoundException)
            {
                return NotFound($"История с ID {id} не найдена");
            }
        }
    }
}

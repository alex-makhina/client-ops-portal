using ClientOpsPortal.Services.SubscriptionHistory.Contracts.DTOs;
using ClientOpsPortal.Services.SubscriptionHistory.Contracts.Exceptions;
using ClientOpsPortal.Services.SubscriptionHistory.Services;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace ClientOpsPortal.Services.Subscription.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class SubscriptionHistoriesController : ControllerBase
    {
        private readonly SubscriptionHistoryService _historyService;
        private readonly IValidator<CreateSubscriptionHistoryDto> _createValidator;
        private readonly IValidator<UpdateSubscriptionHistoryDto> _updateValidator;

        public SubscriptionHistoriesController(
            SubscriptionHistoryService historyService,
            IValidator<CreateSubscriptionHistoryDto> createValidator,
            IValidator<UpdateSubscriptionHistoryDto> updateValidator)
        {
            _historyService = historyService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct = default)
        {
            var histories = await _historyService.GetAllSubscriptionHistoryAsync(ct);
            return Ok(histories);
        }

        [HttpGet("{id}", Name = "GetHistoryById")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
        {
            var history = await _historyService.GetSubscriptionHistoryByIdAsync(id, ct);
            if (history == null)
                return NotFound($"История с ID {id} не найдена");
            return Ok(history);
        }

        [HttpGet("by-subscription/{subscriptionId}")]
        public async Task<IActionResult> GetBySubscription(Guid subscriptionId, CancellationToken ct = default)
        {
            var histories = await _historyService.GetSubscriptionHistoryWhereAsync(h => h.SubscriptionId == subscriptionId, ct);
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
            var validationResult = await _createValidator.ValidateAsync(createDto, ct);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            try
            {
                var history = await _historyService.CreateSubscriptionHistoryAsync(createDto, ct);
                return CreatedAtRoute("GetHistoryById", new { id = history.Id }, history);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateSubscriptionHistoryDto updateDto, CancellationToken ct = default)
        {
            var validationResult = await _updateValidator.ValidateAsync(updateDto, ct);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            try
            {
                var history = await _historyService.UpdateSubscriptionHistoryAsync(id, updateDto, ct);
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
                await _historyService.DeleteSubscriptionHistoryAsync(id, ct);
                return NoContent();
            }
            catch (EntityNotFoundException)
            {
                return NotFound($"История с ID {id} не найдена");
            }
        }
    }
}

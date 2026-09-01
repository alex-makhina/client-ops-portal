using ClientOpsPortal.Services.SubscriptionHistory.Contracts.DTOs;
using ClientOpsPortal.Services.SubscriptionHistory.Contracts.Exceptions;
using ClientOpsPortal.Services.SubscriptionHistory.Services;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace ClientOpsPortal.Services.SubscriptionHistory.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class SubscriptionHistoryStepController : ControllerBase
    {
        private readonly SubscriptionHistoryStepService _historyStepService;
        private readonly IValidator<CreateSubscriptionHistoryStepDto> _createValidator;
        private readonly IValidator<UpdateSubscriptionHistoryStepDto> _updateValidator;

        public SubscriptionHistoryStepController(
            SubscriptionHistoryStepService historyStepService,
            IValidator<CreateSubscriptionHistoryStepDto> createValidator,
            IValidator<UpdateSubscriptionHistoryStepDto> updateValidator)
        {
            _historyStepService = historyStepService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct = default)
        {
            var services = await _historyStepService.GetAllSubscriptionHistoryStepAsync(ct);
            return Ok(services);
        }

        [HttpGet("{id}", Name = "GetStepById")]
        public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var step = await _historyStepService.GetSubscriptionHistoryStepByIdAsync(id, ct);
            if (step == null)
                return NotFound($"Шаг истории с ID {id} не найден");
            return Ok(step);
        }

        [HttpGet("by-history/{historyId}")]
        public async Task<IActionResult> GetByHistory(Guid historyId, CancellationToken ct = default)
        {
            var steps = await _historyStepService.GetStepsByHistoryAsync(historyId, ct);
            return Ok(steps);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateSubscriptionHistoryStepDto createDto, CancellationToken ct = default)
        {
            var validationResult = await _createValidator.ValidateAsync(createDto, ct);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            try
            {
                var step = await _historyStepService.CreateSubscriptionHistoryStepAsync(createDto, ct);
                return CreatedAtRoute("GetStepById", new { id = step.Id }, step);
            }
            catch (EntityNotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateSubscriptionHistoryStepDto updateDto, CancellationToken ct = default)
        {
            var validationResult = await _updateValidator.ValidateAsync(updateDto, ct);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);

            try
            {
                var step = await _historyStepService.UpdateSubscriptionHistoryStepAsync(id, updateDto, ct);
                if (step == null)
                    return NotFound($"Шаг истории с ID {id} не найден");
                return Ok(step);
            }
            catch (EntityNotFoundException)
            {
                return NotFound($"Шаг истории с ID {id} не найден");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
        {
            try
            {
                await _historyStepService.DeleteSubscriptionHistoryStepAsync(id, ct);
                return NoContent();
            }
            catch (EntityNotFoundException)
            {
                return NotFound($"Шаг истории с ID {id} не найден");
            }
        }
    }
}

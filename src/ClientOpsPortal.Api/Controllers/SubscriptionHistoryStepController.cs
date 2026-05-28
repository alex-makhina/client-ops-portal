using ClientOpsPortal.Application.DTOs;
using ClientOpsPortal.Application.Interfaces;
using ClientOpsPortal.Application.Services;
using ClientOpsPortal.Domain.Exceptions;
using ClientOpsPortal.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClientOpsPortal.Api.Controllers
{
    [Authorize]
    public class SubscriptionHistoryStepsController : BaseController
    {
        private readonly ISubscriptionHistoryStepService _stepService;

        public SubscriptionHistoryStepsController(
            ISubscriptionHistoryStepService stepService,
            ICurrentUserService currentUserService)
            : base(currentUserService)
        {
            _stepService = stepService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool withIncludes = false, CancellationToken ct = default)
        {
            var services = await _stepService.GetAllAsync(withIncludes, ct);
            return Ok(services);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var step = await _stepService.GetByIdAsync(id, false, ct);
            if (step == null)
                return NotFound($"Шаг истории с ID {id} не найден");
            return Ok(step);
        }

        [HttpGet("by-history/{historyId}")]
        public async Task<IActionResult> GetByHistory(Guid historyId, CancellationToken ct = default)
        {
            var steps = await _stepService.GetStepsByHistoryAsync(historyId, ct);
            return Ok(steps);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateSubscriptionHistoryStepDto createDto, CancellationToken ct = default)
        {
            try
            {
                var step = await _stepService.CreateAsync(createDto, ct);
                return CreatedAtAction(nameof(GetByIdAsync), new { id = step.Id }, step);
            }
            catch (EntityNotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateSubscriptionHistoryStepDto updateDto, CancellationToken ct = default)
        {
            try
            {
                var step = await _stepService.UpdateAsync(id, updateDto, ct);
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
                await _stepService.DeleteAsync(id, ct);
                return NoContent();
            }
            catch (EntityNotFoundException)
            {
                return NotFound($"Шаг истории с ID {id} не найден");
            }
        }
    }
}

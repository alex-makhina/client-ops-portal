using ClientOpsPortal.Application.DTOs;
using ClientOpsPortal.Application.Interfaces;
using ClientOpsPortal.Domain.Entities;
using ClientOpsPortal.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace ClientOpsPortal.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TariffPlansController : ControllerBase
    {
        private readonly ITariffPlanService _tariffPlanService;

        public TariffPlansController(ITariffPlanService tariffPlanService)
        {
            _tariffPlanService = tariffPlanService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool withIncludes = false, CancellationToken ct = default)
        {
            var tariffPlans = await _tariffPlanService.GetAllAsync(withIncludes, ct);
            return Ok(tariffPlans);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, [FromQuery] bool withIncludes = true, CancellationToken ct = default)
        {
            var tariffPlan = await _tariffPlanService.GetByIdAsync(id, withIncludes, ct);
            if (tariffPlan == null)
                return NotFound($"Тарифный план с ID {id} не найден");
            return Ok(tariffPlan);
        }

        [HttpGet("by-service/{serviceId}")]
        public async Task<IActionResult> GetByService(Guid serviceId, CancellationToken ct = default)
        {
            var tariffPlans = await _tariffPlanService.GetTariffPlansByServiceAsync(serviceId, ct);
            return Ok(tariffPlans);
        }

        [HttpGet("by-service/active/{serviceId}")]
        public async Task<IActionResult> GetActiveByService(Guid serviceId, CancellationToken ct = default)
        {
            var tariffPlans = await _tariffPlanService.GetActiveTariffPlansByServiceAsync(serviceId, ct);
            return Ok(tariffPlans);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateTariffPlanDto createDto, CancellationToken ct = default)
        {
            try
            {
                var tariffPlan = await _tariffPlanService.CreateAsync(createDto, ct);
                return CreatedAtAction(nameof(GetById), new { id = tariffPlan.Id }, tariffPlan);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateTariffPlanDto updateDto, CancellationToken ct = default)
        {
            try
            {
                var tariffPlan = await _tariffPlanService.UpdateAsync(id, updateDto, ct);
                return Ok(tariffPlan);
            }
            catch (EntityNotFoundException)
            {
                return NotFound($"Тарифный план с ID {id} не найден");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
        {
            try
            {
                await _tariffPlanService.DeleteAsync(id, ct);
                return NoContent();
            }
            catch (EntityNotFoundException)
            {
                return NotFound($"Тарифный план с ID {id} не найден");
            }
        }
    }
}
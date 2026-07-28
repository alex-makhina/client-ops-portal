using ClientOpsPortal.Services.Directory.Contracts.DTOs;
using ClientOpsPortal.Services.Directory.Contracts.Exceptions;
using ClientOpsPortal.Services.Directory.Services;
using Microsoft.AspNetCore.Mvc;

namespace ClientOpsPortal.Services.Directory.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class TariffPlansController : ControllerBase
    {
        private readonly DirectoryService _service;

        public TariffPlansController(DirectoryService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool withIncludes = false, CancellationToken ct = default)
        {
            var tariffs = await _service.GetAllTariffPlansAsync(withIncludes, ct);
            return Ok(tariffs);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, [FromQuery] bool withIncludes = true, CancellationToken ct = default)
        {
            var tariff = await _service.GetTariffPlanByIdAsync(id, withIncludes, ct);
            if (tariff == null)
                return NotFound($"Тарифный план с ID {id} не найден");
            return Ok(tariff);
        }

        [HttpGet("by-service/{serviceId}")]
        public async Task<IActionResult> GetByService(Guid serviceId, CancellationToken ct = default)
        {
            var tariffs = await _service.GetTariffPlansByServiceAsync(serviceId, ct);
            return Ok(tariffs);
        }

        [HttpGet("by-service/active/{serviceId}")]
        public async Task<IActionResult> GetActiveByService(Guid serviceId, CancellationToken ct = default)
        {
            var tariffs = await _service.GetActiveTariffPlansByServiceAsync(serviceId, ct);
            return Ok(tariffs);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateTariffPlanDto createDto, CancellationToken ct = default)
        {
            try
            {
                var tariff = await _service.CreateTariffPlanAsync(createDto, ct);
                return CreatedAtAction(nameof(GetById), new { id = tariff.Id }, tariff);
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
                var tariff = await _service.UpdateTariffPlanAsync(id, updateDto, ct);
                return Ok(tariff);
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
                await _service.DeleteTariffPlanAsync(id, ct);
                return NoContent();
            }
            catch (EntityNotFoundException)
            {
                return NotFound($"Тарифный план с ID {id} не найден");
            }
        }
    }
}

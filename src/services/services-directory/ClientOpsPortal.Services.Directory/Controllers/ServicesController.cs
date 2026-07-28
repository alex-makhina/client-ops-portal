using ClientOpsPortal.Services.Directory.Contracts.DTOs;
using ClientOpsPortal.Services.Directory.Contracts.Exceptions;
using ClientOpsPortal.Services.Directory.Services;
using Microsoft.AspNetCore.Mvc;

namespace ClientOpsPortal.Services.Directory.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ServicesController : ControllerBase
    {
        private readonly DirectoryService _service;

        public ServicesController(DirectoryService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool withIncludes = false, CancellationToken ct = default)
        {
            var services = await _service.GetAllServicesAsync(withIncludes, ct);
            return Ok(services);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, [FromQuery] bool withIncludes = true, CancellationToken ct = default)
        {
            var service = await _service.GetServiceByIdAsync(id, withIncludes, ct);
            if (service == null)
                return NotFound($"Услуга с ID {id} не найдена");
            return Ok(service);
        }

        [HttpGet("full/{id}")]
        public async Task<IActionResult> GetFullServiceData(Guid id, CancellationToken ct = default)
        {
            var service = await _service.GetFullServiceDataAsync(id, ct);
            if (service == null)
                return NotFound($"Услуга с ID {id} не найдена");
            return Ok(service);
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveServices(CancellationToken ct = default)
        {
            var services = await _service.GetActiveServicesAsync(ct);
            return Ok(services);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateServiceDto createDto, CancellationToken ct = default)
        {
            try
            {
                var service = await _service.CreateServiceAsync(createDto, ct);
                return CreatedAtAction(nameof(GetById), new { id = service.Id }, service);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateServiceDto updateDto, CancellationToken ct = default)
        {
            try
            {
                var service = await _service.UpdateServiceAsync(id, updateDto, ct);
                return Ok(service);
            }
            catch (EntityNotFoundException)
            {
                return NotFound($"Услуга с ID {id} не найдена");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
        {
            try
            {
                await _service.DeleteServiceAsync(id, ct);
                return NoContent();
            }
            catch (EntityNotFoundException)
            {
                return NotFound($"Услуга с ID {id} не найдена");
            }
        }
    }
}

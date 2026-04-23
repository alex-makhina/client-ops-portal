using ClientOpsPortal.Application.DTOs;
using ClientOpsPortal.Application.Interfaces;
using ClientOpsPortal.Domain.Entities;
using ClientOpsPortal.Domain.Exceptions;
using ClientOpsPortal.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClientOpsPortal.Api.Controllers
{
    [Authorize]
    public class ServicesController : BaseController
    {
        private readonly IServiceService _serviceService;

        public ServicesController(
            IServiceService serviceService,
            ICurrentUserService currentUserService)
            : base(currentUserService)
        {
            _serviceService = serviceService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool withIncludes = false, CancellationToken ct = default)
        {
            var services = await _serviceService.GetAllAsync(withIncludes, ct);
            return Ok(services);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, [FromQuery] bool withIncludes = true, CancellationToken ct = default)
        {
            var service = await _serviceService.GetByIdAsync(id, withIncludes, ct);
            if (service == null)
                return NotFound($"Услуга с ID {id} не найдена");
            return Ok(service);
        }

        [HttpGet("full/{id}")]
        public async Task<IActionResult> GetFullServiceData(Guid id, CancellationToken ct = default)
        {
            var service = await _serviceService.GetFullServiceDataAsync(id, ct);
            if (service == null)
                return NotFound($"Услуга с ID {id} не найдена");
            return Ok(service);
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveServices(CancellationToken ct = default)
        {
            var services = await _serviceService.GetActiveServicesAsync(ct);
            return Ok(services);
        }

        [HttpPost]
        [Authorize(Roles = "ServiceManager")]
        public async Task<IActionResult> Create(CreateServiceDto createDto, CancellationToken ct = default)
        {
            try
            {
                var service = await _serviceService.CreateAsync(createDto, ct);
                return CreatedAtAction(nameof(GetById), new { id = service.Id }, service);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ServiceManager")]
        public async Task<IActionResult> Update(Guid id, UpdateServiceDto updateDto, CancellationToken ct = default)
        {
            try
            {
                var service = await _serviceService.UpdateAsync(id, updateDto, ct);
                return Ok(service);
            }
            catch (EntityNotFoundException)
            {
                return NotFound($"Услуга с ID {id} не найдена");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ServiceManager")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
        {
            try
            {
                await _serviceService.DeleteAsync(id, ct);
                return NoContent();
            }
            catch (EntityNotFoundException)
            {
                return NotFound($"Услуга с ID {id} не найдена");
            }
        }
    }
}

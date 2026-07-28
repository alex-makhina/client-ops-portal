using ClientOpsPortal.Application.DTOs;
using ClientOpsPortal.Domain.Interfaces.Services;
using ClientOpsPortal.Services.Directory.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClientOpsPortal.Api.Controllers
{
    [Authorize]
    public class ServicesController : BaseController
    {
        private readonly IServicesDirectoryClient _directoryClient;

        public ServicesController(
            IServicesDirectoryClient directoryClient,
            ICurrentUserService currentUserService)
            : base(currentUserService)
        {
            _directoryClient = directoryClient;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool withIncludes = false, CancellationToken ct = default)
        {
            var services = await _directoryClient.GetAllServicesAsync(withIncludes, ct);
            return Ok(services);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, [FromQuery] bool withIncludes = true, CancellationToken ct = default)
        {
            var service = await _directoryClient.GetServiceByIdAsync(id, withIncludes, ct);
            if (service == null)
                return NotFound($"Услуга с ID {id} не найдена");
            return Ok(service);
        }

        [HttpGet("full/{id}")]
        public async Task<IActionResult> GetFullServiceData(Guid id, CancellationToken ct = default)
        {
            var service = await _directoryClient.GetFullServiceDataAsync(id, ct);
            if (service == null)
                return NotFound($"Услуга с ID {id} не найдена");
            return Ok(service);
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveServices(CancellationToken ct = default)
        {
            var services = await _directoryClient.GetActiveServicesAsync(ct);
            return Ok(services);
        }

        [HttpPost]
        [Authorize(Roles = "ServiceManager")]
        public async Task<IActionResult> Create(ClientOpsPortal.Services.Directory.Contracts.DTOs.CreateServiceDto createDto, CancellationToken ct = default)
        {
            try
            {
                var service = await _directoryClient.CreateServiceAsync(createDto, ct);
                return CreatedAtAction(nameof(GetById), new { id = service.Id }, service);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ServiceManager")]
        public async Task<IActionResult> Update(Guid id, ClientOpsPortal.Services.Directory.Contracts.DTOs.UpdateServiceDto updateDto, CancellationToken ct = default)
        {
            try
            {
                var service = await _directoryClient.UpdateServiceAsync(id, updateDto, ct);
                return Ok(service);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
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
                await _directoryClient.DeleteServiceAsync(id, ct);
                return NoContent();
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound($"Услуга с ID {id} не найдена");
            }
        }
    }
}

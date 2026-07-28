using ClientOpsPortal.Domain.Interfaces.Services;
using ClientOpsPortal.Services.Directory.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClientOpsPortal.Api.Controllers
{
    [Authorize]
    public class TariffPlansController : BaseController
    {
        private readonly IServicesDirectoryClient _directoryClient;

        public TariffPlansController(
            IServicesDirectoryClient directoryClient,
            ICurrentUserService currentUserService)
            : base(currentUserService)
        {
            _directoryClient = directoryClient;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool withIncludes = false, CancellationToken ct = default)
        {
            var tariffs = await _directoryClient.GetAllTariffPlansAsync(withIncludes, ct);
            return Ok(tariffs);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, [FromQuery] bool withIncludes = true, CancellationToken ct = default)
        {
            var tariff = await _directoryClient.GetTariffPlanByIdAsync(id, withIncludes, ct);
            if (tariff == null)
                return NotFound($"Тарифный план с ID {id} не найден");
            return Ok(tariff);
        }

        [HttpGet("by-service/{serviceId}")]
        public async Task<IActionResult> GetByService(Guid serviceId, CancellationToken ct = default)
        {
            var tariffs = await _directoryClient.GetTariffPlansByServiceAsync(serviceId, ct);
            return Ok(tariffs);
        }

        [HttpGet("by-service/active/{serviceId}")]
        public async Task<IActionResult> GetActiveByService(Guid serviceId, CancellationToken ct = default)
        {
            var tariffs = await _directoryClient.GetActiveTariffPlansByServiceAsync(serviceId, ct);
            return Ok(tariffs);
        }

        [HttpPost]
        [Authorize(Roles = "ServiceManager")]
        public async Task<IActionResult> Create(ClientOpsPortal.Services.Directory.Contracts.DTOs.CreateTariffPlanDto createDto, CancellationToken ct = default)
        {
            try
            {
                var tariff = await _directoryClient.CreateTariffPlanAsync(createDto, ct);
                return CreatedAtAction(nameof(GetById), new { id = tariff.Id }, tariff);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ServiceManager")]
        public async Task<IActionResult> Update(Guid id, ClientOpsPortal.Services.Directory.Contracts.DTOs.UpdateTariffPlanDto updateDto, CancellationToken ct = default)
        {
            try
            {
                var tariff = await _directoryClient.UpdateTariffPlanAsync(id, updateDto, ct);
                return Ok(tariff);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound($"Тарифный план с ID {id} не найден");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ServiceManager")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
        {
            try
            {
                await _directoryClient.DeleteTariffPlanAsync(id, ct);
                return NoContent();
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound($"Тарифный план с ID {id} не найден");
            }
        }
    }
}

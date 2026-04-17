using ClientOpsPortal.Application.DTOs;
using ClientOpsPortal.Application.Interfaces;
using ClientOpsPortal.Domain.Entities;
using ClientOpsPortal.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace ClientOpsPortal.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AbonentsController : ControllerBase
    {
        private readonly IAbonentService _abonentService;

        public AbonentsController(IAbonentService abonentService)
        {
            _abonentService = abonentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool withIncludes = false, CancellationToken ct = default)
        {
            var abonents = await _abonentService.GetAllAsync(withIncludes, ct);
            return Ok(abonents);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, [FromQuery] bool withIncludes = true, CancellationToken ct = default)
        {
            var abonent = await _abonentService.GetByIdAsync(id, withIncludes, ct);
            if (abonent == null)
                return NotFound($"Абонент с ID {id} не найден");
            return Ok(abonent);
        }

        [HttpGet("by-account/{accountNumber}")]
        public async Task<IActionResult> GetByAccountNumber(string accountNumber, [FromQuery] bool withIncludes = true, CancellationToken ct = default)
        {
            var abonent = await _abonentService.GetWhereAsync(a => a.AccountNumber == accountNumber, false, ct);
            if (abonent == null)
                return NotFound($"Абонент с лицевым счетом {accountNumber} не найден");
            return Ok(abonent);
        }

        [HttpGet("by-contract/{contractNumber}")]
        public async Task<IActionResult> GetByContractNumber(string contractNumber, CancellationToken ct = default)
        {
            var abonent = await _abonentService.GetByContractNumberAsync(contractNumber, ct);
            if (abonent == null)
                return NotFound($"Абонент с договором {contractNumber} не найден");
            return Ok(abonent);
        }

        [HttpGet("search/by-name")]
        public async Task<IActionResult> SearchByName([FromQuery] string searchTerm, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return BadRequest("Search term cannot be empty");

            var result = await _abonentService.SearchByFullNameAsync(searchTerm, ct);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateAbonentDto createDto, CancellationToken ct = default)
        {
            try
            {
                var abonent = await _abonentService.CreateAsync(createDto, ct);
                return CreatedAtAction(nameof(GetById), new { id = abonent.Id }, abonent);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateAbonentDto updateDto, CancellationToken ct = default)
        {
            try
            {
                var abonent = await _abonentService.UpdateAsync(id, updateDto, ct);
                return Ok(abonent);
            }
            catch (EntityNotFoundException)
            {
                return NotFound($"Абонент с ID {id} не найден");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
        {
            try
            {
                await _abonentService.DeleteAsync(id, ct);
                return NoContent();
            }
            catch (EntityNotFoundException)
            {
                return NotFound($"Абонент с ID {id} не найден");
            }
        }
    }
}
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
    public class AbonentsController : BaseController
    {
        private readonly IAbonentService _abonentService;

        public AbonentsController(
            IAbonentService abonentService,
            ICurrentUserService currentUserService)
            : base(currentUserService)
        {
            _abonentService = abonentService;
        }

        [HttpGet]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> GetAll([FromQuery] bool withIncludes = false, CancellationToken ct = default)
        {
            var abonents = await _abonentService.GetAllAsync(withIncludes, ct);
            return Ok(abonents);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> GetById(Guid id, [FromQuery] bool withIncludes = true, CancellationToken ct = default)
        {
            var abonent = await _abonentService.GetByIdAsync(id, withIncludes, ct);
            if (abonent == null)
                return NotFound($"Абонент с ID {id} не найден");
            return Ok(abonent);
        }

        [HttpGet("by-account/{accountNumber}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> GetByAccountNumber(string accountNumber, [FromQuery] bool withIncludes = true, CancellationToken ct = default)
        {
            var abonents = await _abonentService.GetWhereAsync(a => a.AccountNumber == accountNumber, false, ct);
            var abonent = abonents.FirstOrDefault();

            if (abonent == null)
                return NotFound($"Абонент с лицевым счетом {accountNumber} не найден");
            return Ok(abonent);
        }

        [HttpGet("by-contract/{contractNumber}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> GetByContractNumber(string contractNumber, CancellationToken ct = default)
        {
            var abonent = await _abonentService.GetByContractNumberAsync(contractNumber, ct);
            if (abonent == null)
                return NotFound($"Абонент с договором {contractNumber} не найден");
            return Ok(abonent);
        }

        [HttpGet("search/by-name")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> SearchByName([FromQuery] string searchTerm, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return BadRequest("Search term cannot be empty");

            var result = await _abonentService.SearchByFullNameAsync(searchTerm, ct);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
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

        [HttpPost("register")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Register(CreateAbonentDto createDto, CancellationToken ct = default)
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
        [Authorize(Roles = "Manager,Abonent")]
        public async Task<IActionResult> Update(Guid id, UpdateAbonentDto updateDto, CancellationToken ct = default)
        {
            try
            {
                if (User.IsInRole("Abonent"))
                {
                    var existingAbonent = await _abonentService.GetByIdAsync(id, false, ct);
                    if (existingAbonent == null)
                        return NotFound($"Абонент с ID {id} не найден");
                    if (!IsCurrentUserAbonentOwner(existingAbonent.UserId))
                        return Forbid();
                }

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
        [Authorize(Roles = "Manager")]
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

using ClientOpsPortal.Application.DTOs;
using ClientOpsPortal.Application.Interfaces;
using ClientOpsPortal.Domain.Exceptions;
using ClientOpsPortal.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClientOpsPortal.Api.Controllers
{
    [Authorize]
    public class ContractsController : BaseController
    {
        private readonly IContractService _contractService;
        private readonly IAbonentService _abonentService;

        public ContractsController(
            IContractService contractService,
            IAbonentService abonentService,
            ICurrentUserService currentUserService)
            : base(currentUserService)
        {
            _contractService = contractService;
            _abonentService = abonentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool withIncludes = false, CancellationToken ct = default)
        {
            var contracts = await _contractService.GetAllAsync(withIncludes, ct);
            return Ok(contracts);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, [FromQuery] bool withIncludes = true, CancellationToken ct = default)
        {
            var contract = await _contractService.GetByIdAsync(id, withIncludes, ct);
            if (contract == null)
                return NotFound($"Договор с ID {id} не найден");
            return Ok(contract);
        }

        [HttpGet("by-abonent/{abonentId}")]
        public async Task<IActionResult> GetByAbonent(Guid abonentId, [FromQuery] bool withIncludes = true, CancellationToken ct = default)
        {
            if (User.IsInRole("Abonent"))
            {
                var abonent = await _abonentService.GetByIdAsync(abonentId, false, ct);
                if (abonent == null || !IsCurrentUserAbonentOwner(abonent.UserId))
                    return Forbid();
            }

            var contracts = await _contractService.GetShortContractsByAbonentAsync(abonentId, ct);
            return Ok(contracts);
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Create(ContractDataDto createDto, CancellationToken ct = default)
        {
            try
            {
                var contract = await _contractService.CreateAsync(createDto, ct);
                return CreatedAtAction(nameof(GetById), new { id = contract.Id }, contract);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Update(Guid id, UpdateContractDto updateDto, CancellationToken ct = default)
        {
            try
            {
                var contract = await _contractService.UpdateAsync(id, updateDto, ct);
                return Ok(contract);
            }
            catch (EntityNotFoundException)
            {
                return NotFound($"Договор с ID {id} не найден");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
        {
            try
            {
                await _contractService.DeleteAsync(id, ct);
                return NoContent();
            }
            catch (EntityNotFoundException)
            {
                return NotFound($"Договор с ID {id} не найден");
            }
        }
    }
}

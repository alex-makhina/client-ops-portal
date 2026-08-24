using ClientOpsPortal.Application.DTOs;
using ClientOpsPortal.Application.Interfaces;
using ClientOpsPortal.Application.Mappings;
using ClientOpsPortal.Domain.Entities;
using ClientOpsPortal.Domain.Exceptions;
using ClientOpsPortal.Domain.Interfaces.Repositories;
using ClientOpsPortal.Services.Reporting.Contracts.Events;
using MassTransit;
using System.Linq.Expressions;

namespace ClientOpsPortal.Application.Services
{
    public class ContractService : IContractService
    {
        private readonly IGenericRepository<Contract> _contractRepository;
        private readonly IPublishEndpoint _publishEndpoint;

        public ContractService(
            IGenericRepository<Contract> contractRepository,
            IPublishEndpoint publishEndpoint)
        {
            _contractRepository = contractRepository;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<ContractDataDto?> GetByIdAsync(Guid id, bool withIncludes = false, CancellationToken ct = default)
        {
            var contract = await _contractRepository.GetByIdAsync(id, withIncludes, ct);
            return contract?.ToContractDto();
        }

        public async Task<IReadOnlyCollection<ContractDataDto>> GetAllAsync(bool withIncludes = false, CancellationToken ct = default)
        {
            var contracts = await _contractRepository.GetAllAsync(withIncludes, ct);
            return contracts.Select(c => c.ToContractDto()).ToList();
        }

        public async Task<ContractDataDto> CreateAsync(ContractDataDto createDto, CancellationToken ct = default)
        {
            var contract = createDto.ToEntity();
            await _contractRepository.AddAsync(contract, ct);

            await _publishEndpoint.Publish(new ContractCreatedEvent(
                contract.Id,
                contract.ContractNumber,
                contract.AbonentId,
                contract.BeginDate,
                contract.EndDate,
                contract.CreatedAt,
                contract.CreatedBy,
                contract.UpdatedAt,
                contract.UpdatedBy,
                DateTimeOffset.UtcNow
            ), ct);

            return contract.ToContractDto();
        }

        public async Task<ContractDataDto> UpdateAsync(Guid id, UpdateContractDto updateDto, CancellationToken ct = default)
        {
            var contract = await _contractRepository.GetByIdAsync(id, true, ct);
            if (contract == null)
                throw new EntityNotFoundException(typeof(Contract), id);

            var activeSubscriptions = contract.Subscriptions.Where(d => d.EndDate == null || d.EndDate > DateTimeOffset.UtcNow).ToList().Count;

            if (activeSubscriptions > 0)
                throw new InvalidOperationException($"На договоре есть активные подписки");

            updateDto.UpdateEntity(contract);
            await _contractRepository.UpdateAsync(contract, ct);

            await _publishEndpoint.Publish(new ContractUpdatedEvent(
                contract.Id,
                contract.ContractNumber,
                contract.AbonentId,
                contract.BeginDate,
                contract.EndDate,
                contract.CreatedAt,
                contract.CreatedBy,
                contract.UpdatedAt,
                contract.UpdatedBy,
                DateTimeOffset.UtcNow
            ), ct);
            return contract.ToContractDto();
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            await _contractRepository.DeleteAsync(id, ct);

            await _publishEndpoint.Publish(new ContractDeletedEvent(id, DateTimeOffset.UtcNow), ct);
        }

        public async Task<IReadOnlyCollection<ContractDataDto>> GetWhereAsync(Expression<Func<Contract, bool>> predicate, bool withIncludes = false, CancellationToken ct = default)
        {
            var contracts = await _contractRepository.GetWhereAsync(predicate, withIncludes, ct);
            return contracts.Select(c => c.ToContractDto()).ToList();
        }

        public async Task<IReadOnlyCollection<ContractShortDataDto>> GetShortContractsByAbonentAsync(Guid abonentId, CancellationToken ct = default)
        {
            var contracts = await _contractRepository.GetWhereAsync(c => c.AbonentId == abonentId, false, ct);
            return contracts.Select(c => c.ToShortDto()).ToList();
        }
    }
}
using ClientOpsPortal.Services.Reporting.Contracts.Events;
using ClientOpsPortal.Services.Reporting.Data;
using MassTransit;

namespace ClientOpsPortal.Services.Reporting.Consumers;

[EntityName("reporting-contract-queue")]
public class ContractEventConsumer : IConsumer<ContractCreatedEvent>,
                                     IConsumer<ContractUpdatedEvent>,
                                     IConsumer<ContractDeletedEvent>
{
    private readonly ReportsDbContext _context;
    private readonly ILogger<ContractEventConsumer> _logger;

    public ContractEventConsumer(ReportsDbContext context, ILogger<ContractEventConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ContractCreatedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Получено событие ContractCreatedEvent для ContractId: {Id}", message.Id);
        await UpsertContractAsync(message.Id, message.ContractNumber, message.AbonentId, message.BeginDate, message.EndDate, message.CreatedAt, message.CreatedBy, message.UpdatedAt, message.UpdatedBy);
    }

    public async Task Consume(ConsumeContext<ContractUpdatedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Получено событие ContractUpdatedEvent для ContractId: {Id}", message.Id);
        await UpsertContractAsync(message.Id, message.ContractNumber, message.AbonentId, message.BeginDate, message.EndDate, message.CreatedAt, message.CreatedBy, message.UpdatedAt, message.UpdatedBy);
    }

    public async Task Consume(ConsumeContext<ContractDeletedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Получено событие ContractDeletedEvent для ContractId: {Id}", message.Id);

        var contract = await _context.Contracts.FindAsync(message.Id);
        if (contract != null)
        {
            _context.Contracts.Remove(contract);
            await _context.SaveChangesAsync();
        }
    }

    private async Task UpsertContractAsync(
        Guid id, string contractNumber, Guid abonentId, DateTimeOffset beginDate, DateTimeOffset? endDate,
        DateTimeOffset createdAt, string? createdBy, DateTimeOffset? updatedAt, string? updatedBy)
    {
        var existing = await _context.Contracts.FindAsync(id);

        if (existing == null)
        {
            _context.Contracts.Add(new Contracts.Models.Contract
            {
                Id = id,
                ContractNumber = contractNumber,
                AbonentId = abonentId,
                BeginDate = beginDate,
                EndDate = endDate,
                CreatedAt = createdAt,
                CreatedBy = createdBy,
                UpdatedAt = updatedAt,
                UpdatedBy = updatedBy
            });
        }
        else
        {
            existing.ContractNumber = contractNumber;
            existing.AbonentId = abonentId;
            existing.BeginDate = beginDate;
            existing.EndDate = endDate;
            existing.UpdatedAt = updatedAt;
            existing.UpdatedBy = updatedBy;

            _context.Contracts.Update(existing);
        }

        await _context.SaveChangesAsync();
    }
}
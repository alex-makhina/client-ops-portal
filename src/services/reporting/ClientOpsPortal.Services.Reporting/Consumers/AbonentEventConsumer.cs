using ClientOpsPortal.Services.Reporting.Contracts.Events;
using ClientOpsPortal.Services.Reporting.Data;
using MassTransit;

namespace ClientOpsPortal.Services.Reporting.Consumers;

[EntityName("reporting-abonent-queue")]
public class AbonentEventConsumer : IConsumer<AbonentCreatedEvent>,
                                     IConsumer<AbonentUpdatedEvent>,
                                     IConsumer<AbonentDeletedEvent>
{
    private readonly ReportsDbContext _context;
    private readonly ILogger<AbonentEventConsumer> _logger;

    public AbonentEventConsumer(ReportsDbContext context, ILogger<AbonentEventConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AbonentCreatedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Получено событие AbonentCreatedEvent для AbonentId: {Id}", message.Id);

        await UpsertAbonentAsync(
            message.Id,
            message.IdentificationNumber,
            message.FirstName,
            message.LastName,
            message.MiddleName,
            message.UserId,
            message.AccountNumber,
            message.CreatedAt,
            message.CreatedBy,
            message.UpdatedAt,
            message.UpdatedBy
        );
    }

    public async Task Consume(ConsumeContext<AbonentUpdatedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Получено событие AbonentUpdatedEvent для AbonentId: {Id}", message.Id);

        await UpsertAbonentAsync(
            message.Id,
            message.IdentificationNumber,
            message.FirstName,
            message.LastName,
            message.MiddleName,
            message.UserId,
            message.AccountNumber,
            message.CreatedAt,
            message.CreatedBy,
            message.UpdatedAt,
            message.UpdatedBy
        );
    }

    public async Task Consume(ConsumeContext<AbonentDeletedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Получено событие AbonentDeletedEvent для AbonentId: {Id}", message.Id);

        var abonent = await _context.Abonents.FindAsync(message.Id);
        if (abonent != null)
        {
            _context.Abonents.Remove(abonent);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Абонент {Id} удален из reporting-db", message.Id);
        }
    }

    private async Task UpsertAbonentAsync(
        Guid id,
        string identificationNumber,
        string firstName,
        string lastName,
        string? middleName,
        Guid userId,
        string accountNumber,
        DateTimeOffset createdAt,
        string? createdBy,
        DateTimeOffset? updatedAt,
        string? updatedBy)
    {
        var existingAbonent = await _context.Abonents.FindAsync(id);

        if (existingAbonent == null)
        {
            _context.Abonents.Add(new Contracts.Models.Abonent
            {
                Id = id,
                IdentificationNumber = identificationNumber,
                FirstName = firstName,
                LastName = lastName,
                MiddleName = middleName,
                UserId = userId,
                AccountNumber = accountNumber,
                CreatedAt = createdAt,
                CreatedBy = createdBy,
                UpdatedAt = updatedAt,
                UpdatedBy = updatedBy
            });
        }
        else
        {
            existingAbonent.IdentificationNumber = identificationNumber;
            existingAbonent.FirstName = firstName;
            existingAbonent.LastName = lastName;
            existingAbonent.MiddleName = middleName;
            existingAbonent.UserId = userId;
            existingAbonent.AccountNumber = accountNumber;
            existingAbonent.CreatedAt = createdAt;
            existingAbonent.CreatedBy = createdBy;
            existingAbonent.UpdatedAt = updatedAt;
            existingAbonent.UpdatedBy = updatedBy;

            _context.Abonents.Update(existingAbonent);
        }

        await _context.SaveChangesAsync();
    }
}
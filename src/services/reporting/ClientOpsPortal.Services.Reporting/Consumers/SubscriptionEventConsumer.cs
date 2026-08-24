using ClientOpsPortal.Services.Reporting.Contracts.Events;
using ClientOpsPortal.Services.Reporting.Data;
using MassTransit;

namespace ClientOpsPortal.Services.Reporting.Consumers;

public class SubscriptionEventConsumer : IConsumer<SubscriptionCreatedEvent>,
                                         IConsumer<SubscriptionUpdatedEvent>,
                                         IConsumer<SubscriptionDeletedEvent>
{
    private readonly ReportsDbContext _context;
    private readonly ILogger<SubscriptionEventConsumer> _logger;

    public SubscriptionEventConsumer(ReportsDbContext context, ILogger<SubscriptionEventConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SubscriptionCreatedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Получено событие SubscriptionCreatedEvent для SubscriptionId: {Id}", message.Id);
        await UpsertSubscriptionAsync(message);
    }

    public async Task Consume(ConsumeContext<SubscriptionUpdatedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Получено событие SubscriptionUpdatedEvent для SubscriptionId: {Id}", message.Id);
        await UpsertSubscriptionAsync(message);
    }

    public async Task Consume(ConsumeContext<SubscriptionDeletedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Получено событие SubscriptionDeletedEvent для SubscriptionId: {Id}", message.Id);

        var subscription = await _context.Subscriptions.FindAsync(message.Id);
        if (subscription != null)
        {
            _context.Subscriptions.Remove(subscription);
            await _context.SaveChangesAsync();
        }
    }

    private async Task UpsertSubscriptionAsync(dynamic message)
    {
        var existing = await _context.Subscriptions.FindAsync(message.Id);

        if (existing == null)
        {
            _context.Subscriptions.Add(new Contracts.Models.Subscription
            {
                Id = message.Id,
                ContractId = message.ContractId,
                ServiceId = message.ServiceId,
                TariffPlanId = message.TariffPlanId,
                BeginDate = message.BeginDate,
                EndDate = message.EndDate,
                CreatedAt = message.CreatedAt,
                CreatedBy = message.CreatedBy,
                UpdatedAt = message.UpdatedAt,
                UpdatedBy = message.UpdatedBy
            });
        }
        else
        {
            existing.ContractId = message.ContractId;
            existing.ServiceId = message.ServiceId;
            existing.TariffPlanId = message.TariffPlanId;
            existing.BeginDate = message.BeginDate;
            existing.EndDate = message.EndDate;
            existing.UpdatedAt = message.UpdatedAt;
            existing.UpdatedBy = message.UpdatedBy;

            _context.Subscriptions.Update(existing);
        }

        await _context.SaveChangesAsync();
    }
}
using ClientOpsPortal.Services.Reporting.Contracts.Events;
using ClientOpsPortal.Services.Reporting.Data;
using MassTransit;

namespace ClientOpsPortal.Services.Reporting.Consumers;

[EntityName("reporting-tariffplan-queue")]
public class TariffPlanEventConsumer : IConsumer<TariffPlanCreatedEvent>,
                                       IConsumer<TariffPlanUpdatedEvent>,
                                       IConsumer<TariffPlanDeletedEvent>
{
    private readonly ReportsDbContext _context;
    private readonly ILogger<TariffPlanEventConsumer> _logger;

    public TariffPlanEventConsumer(ReportsDbContext context, ILogger<TariffPlanEventConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<TariffPlanCreatedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Получено событие TariffPlanCreatedEvent для TariffPlanId: {Id}", message.Id);
        await UpsertTariffPlanAsync(message.Id, message.Name, message.Description, message.Price, message.ServiceId, message.BeginDate, message.EndDate);
    }

    public async Task Consume(ConsumeContext<TariffPlanUpdatedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Получено событие TariffPlanUpdatedEvent для TariffPlanId: {Id}", message.Id);
        await UpsertTariffPlanAsync(message.Id, message.Name, message.Description, message.Price, message.ServiceId, message.BeginDate, message.EndDate);
    }

    public async Task Consume(ConsumeContext<TariffPlanDeletedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Получено событие TariffPlanDeletedEvent для TariffPlanId: {Id}", message.Id);

        var tariffPlan = await _context.TariffPlans.FindAsync(message.Id);
        if (tariffPlan != null)
        {
            _context.TariffPlans.Remove(tariffPlan);
            await _context.SaveChangesAsync();
        }
    }

    private async Task UpsertTariffPlanAsync(Guid id, string name, string description, decimal price, Guid serviceId, DateTimeOffset beginDate, DateTimeOffset? endDate)
    {
        var existing = await _context.TariffPlans.FindAsync(id);

        if (existing == null)
        {
            _context.TariffPlans.Add(new Contracts.Models.TariffPlan
            {
                Id = id,
                Name = name,
                Description = description,
                Price = price,
                ServiceId = serviceId,
                BeginDate = beginDate,
                EndDate = endDate
            });
        }
        else
        {
            existing.Name = name;
            existing.Description = description;
            existing.Price = price;
            existing.ServiceId = serviceId;
            existing.BeginDate = beginDate;
            existing.EndDate = endDate;

            _context.TariffPlans.Update(existing);
        }

        await _context.SaveChangesAsync();
    }
}
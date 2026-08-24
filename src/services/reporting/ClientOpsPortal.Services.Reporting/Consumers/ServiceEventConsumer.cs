using ClientOpsPortal.Services.Reporting.Contracts.Events;
using ClientOpsPortal.Services.Reporting.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ClientOpsPortal.Services.Reporting.Consumers;

public class ServiceEventConsumer : IConsumer<ServiceCreatedEvent>,
                                    IConsumer<ServiceUpdatedEvent>,
                                    IConsumer<ServiceDeletedEvent>
{
    private readonly ReportsDbContext _context;
    private readonly ILogger<ServiceEventConsumer> _logger;

    public ServiceEventConsumer(ReportsDbContext context, ILogger<ServiceEventConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ServiceCreatedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Получено событие ServiceCreatedEvent для ServiceId: {Id}", message.Id);
        await SyncServiceAndTariffsAsync(
            message.Id, message.Name, message.Description, message.BeginDate, message.EndDate,
            message.CreatedAt, message.CreatedBy, message.UpdatedAt, message.UpdatedBy, message.TariffPlans
        );
    }

    public async Task Consume(ConsumeContext<ServiceUpdatedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Получено событие ServiceUpdatedEvent для ServiceId: {Id}", message.Id);
        await SyncServiceAndTariffsAsync(
            message.Id, message.Name, message.Description, message.BeginDate, message.EndDate,
            message.CreatedAt, message.CreatedBy, message.UpdatedAt, message.UpdatedBy, message.TariffPlans
        );
    }

    public async Task Consume(ConsumeContext<ServiceDeletedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Получено событие ServiceDeletedEvent для ServiceId: {Id}", message.Id);

        var service = await _context.Services.FindAsync(message.Id);
        if (service != null)
        {
            var tariffs = await _context.TariffPlans.Where(t => t.ServiceId == message.Id).ToListAsync();
            if (tariffs.Any()) _context.TariffPlans.RemoveRange(tariffs);

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();
        }
    }

    private async Task SyncServiceAndTariffsAsync(
        Guid id, string name, string description, DateTimeOffset beginDate, DateTimeOffset? endDate,
        DateTimeOffset createdAt, string? createdBy, DateTimeOffset? updatedAt, string? updatedBy,
        List<TariffPlanSnapshot> tariffPlans)
    {
        var existingService = await _context.Services.FindAsync(id);
        if (existingService == null)
        {
            _context.Services.Add(new Contracts.Models.Service
            {
                Id = id,
                Name = name,
                Description = description,
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
            existingService.Name = name;
            existingService.Description = description;
            existingService.BeginDate = beginDate;
            existingService.EndDate = endDate;
            existingService.UpdatedAt = updatedAt;
            existingService.UpdatedBy = updatedBy;
            _context.Services.Update(existingService);
        }

        var existingTariffs = await _context.TariffPlans.Where(t => t.ServiceId == id).ToListAsync();
        var existingIds = existingTariffs.Select(t => t.Id).ToHashSet();
        var incomingIds = tariffPlans.Select(t => t.Id).ToHashSet();
        var toDelete = existingTariffs.Where(t => !incomingIds.Contains(t.Id)).ToList();
        if (toDelete.Any())
        {
            _context.TariffPlans.RemoveRange(toDelete);
        }

        foreach (var tp in tariffPlans)
        {
            var existingTp = existingTariffs.FirstOrDefault(t => t.Id == tp.Id);
            if (existingTp == null)
            {
                _context.TariffPlans.Add(new Contracts.Models.TariffPlan
                {
                    Id = tp.Id,
                    ServiceId = tp.ServiceId,
                    Name = tp.Name,
                    Description = tp.Description,
                    Price = tp.Price,
                    BeginDate = tp.BeginDate,
                    EndDate = tp.EndDate
                });
            }
            else
            {
                existingTp.Name = tp.Name;
                existingTp.Description = tp.Description;
                existingTp.Price = tp.Price;
                existingTp.ServiceId = tp.ServiceId;
                existingTp.BeginDate = tp.BeginDate;
                existingTp.EndDate = tp.EndDate;
                _context.TariffPlans.Update(existingTp);
            }
        }

        await _context.SaveChangesAsync();
    }
}
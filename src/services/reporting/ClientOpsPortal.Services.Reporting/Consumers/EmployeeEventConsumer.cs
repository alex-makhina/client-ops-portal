using ClientOpsPortal.Contracts.Events;
using ClientOpsPortal.Services.Reporting.Data;
using MassTransit;

namespace ClientOpsPortal.Services.Reporting.Consumers;

[EntityName("reporting-employee-queue")]
public class EmployeeEventConsumer : IConsumer<EmployeeCreatedEvent>,
                                     IConsumer<EmployeeUpdatedEvent>,
                                     IConsumer<EmployeeDeletedEvent>
{
    private readonly ReportsDbContext _context;
    private readonly ILogger<EmployeeEventConsumer> _logger;

    public EmployeeEventConsumer(ReportsDbContext context, ILogger<EmployeeEventConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<EmployeeCreatedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Получено событие EmployeeCreatedEvent для EmployeeId: {Id}", message.Id);
        await UpsertEmployeeAsync(message);
    }

    public async Task Consume(ConsumeContext<EmployeeUpdatedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Получено событие EmployeeUpdatedEvent для EmployeeId: {Id}", message.Id);
        await UpsertEmployeeAsync(message);
    }

    public async Task Consume(ConsumeContext<EmployeeDeletedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Получено событие EmployeeDeletedEvent для EmployeeId: {Id}", message.Id);

        var employee = await _context.Employees.FindAsync(message.Id);
        if (employee != null)
        {
            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
        }
    }

    private async Task UpsertEmployeeAsync(dynamic message)
    {
        var existing = await _context.Employees.FindAsync(message.Id);

        if (existing == null)
        {
            _context.Employees.Add(new Contracts.Models.Employee
            {
                Id = message.Id,
                StaffNumber = message.StaffNumber,
                FirstName = message.FirstName,
                LastName = message.LastName,
                MiddleName = message.MiddleName,
                UserId = message.UserId,
                Post = message.Post,
                Department = message.Department,
                IsActive = message.IsActive
            });
        }
        else
        {
            existing.StaffNumber = message.StaffNumber;
            existing.FirstName = message.FirstName;
            existing.LastName = message.LastName;
            existing.MiddleName = message.MiddleName;
            existing.UserId = message.UserId;
            existing.Post = message.Post;
            existing.Department = message.Department;
            existing.IsActive = message.IsActive;

            _context.Employees.Update(existing);
        }

        await _context.SaveChangesAsync();
    }
}
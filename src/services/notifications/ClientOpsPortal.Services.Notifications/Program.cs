using ClientOpsPortal.Services.Notifications.Consumers;
using ClientOpsPortal.Services.Notifications.Contracts;
using ClientOpsPortal.Services.Notifications.Notifiers;
using ClientOpsPortal.Services.Notifications.Client;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddScoped<INotificationPublisher, MassTransitNotificationPublisher>();

// Notifiers (in order: email first, console as fallback for dev visibility)
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<INotifier, EmailNotifier>();
builder.Services.AddScoped<INotifier, ConsoleNotifier>();

// MassTransit consumer bus
var rabbit = builder.Configuration.GetSection("RabbitMq").Get<ClientOpsPortal.Services.Notifications.Client.RabbitMqOptions>() ?? new();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<NotificationMessageConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(new Uri($"amqp://{rabbit.Host}:{rabbit.Port}{rabbit.VirtualHost}"), h =>
        {
            h.Username(rabbit.Username);
            h.Password(rabbit.Password);
        });

        cfg.ReceiveEndpoint("notifications", e =>
        {
            e.ConfigureConsumer<NotificationMessageConsumer>(context);
        });
    });
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.MapControllers();

app.Run();
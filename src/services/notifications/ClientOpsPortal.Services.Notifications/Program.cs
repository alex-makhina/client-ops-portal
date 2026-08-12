using ClientOpsPortal.Services.Notifications.Consumers;
using ClientOpsPortal.Services.Notifications.Contracts;
using ClientOpsPortal.Services.Notifications.Notifiers;
using ClientOpsPortal.Services.Notifications.Client;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddScoped<INotificationPublisher, MassTransitNotificationPublisher>();

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<INotifier, EmailNotifier>();
builder.Services.AddScoped<INotifier, ConsoleNotifier>();

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

var jwksUrl = builder.Configuration["Jwt:JwksUrl"]
    ?? "http://localhost:5110/.well-known/jwks";
var issuer = builder.Configuration["Jwt:Issuer"] ?? "http://localhost:5110";
var audience = builder.Configuration["Jwt:Audience"] ?? "ClientOpsPortalClient";
var jwksClient = new HttpClient();
Task<SecurityKey[]> keysTask = null!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true,
            IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
            {
                if (keysTask is null)
                {
                    keysTask = jwksClient.GetStringAsync(jwksUrl)
                        .ContinueWith(t => (SecurityKey[])JsonWebKeySet.Create(t.Result).Keys.Cast<SecurityKey>().ToArray());
                }

                return keysTask.GetAwaiter().GetResult();
            }
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

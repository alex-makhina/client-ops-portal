using ClientOpsPortal.Services.Reporting.Consumers;
using ClientOpsPortal.Services.Reporting.Data;
using ClientOpsPortal.Services.Reporting.Services;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<ReportsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ReportingDb")));

builder.Services.AddScoped<ReportsRepository>();
builder.Services.AddScoped<ReportsService>();

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

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<AbonentEventConsumer>();
    x.AddConsumer<ContractEventConsumer>();
    x.AddConsumer<EmployeeEventConsumer>();
    x.AddConsumer<TariffPlanEventConsumer>();
    x.AddConsumer<ServiceEventConsumer>();
    x.AddConsumer<SubscriptionEventConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var host = builder.Configuration["RabbitMq:Host"] ?? "rabbitmq";
        var username = builder.Configuration["RabbitMq:Username"] ?? "guest";
        var password = builder.Configuration["RabbitMq:Password"] ?? "guest";

        cfg.Host(host, h =>
        {
            h.Username(username);
            h.Password(password);
        });

        cfg.ReceiveEndpoint("reporting-abonent-queue", e => e.ConfigureConsumer<AbonentEventConsumer>(context));
        cfg.ReceiveEndpoint("reporting-contract-queue", e => e.ConfigureConsumer<ContractEventConsumer>(context));
        cfg.ReceiveEndpoint("reporting-employee-queue", e => e.ConfigureConsumer<EmployeeEventConsumer>(context));
        cfg.ReceiveEndpoint("reporting-service-queue", e => e.ConfigureConsumer<ServiceEventConsumer>(context));
        cfg.ReceiveEndpoint("reporting-tariffplan-queue", e => e.ConfigureConsumer<TariffPlanEventConsumer>(context));
        cfg.ReceiveEndpoint("reporting-subscription-queue", e => e.ConfigureConsumer<SubscriptionEventConsumer>(context));
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ReportsDbContext>();
    db.Database.EnsureCreated();
}

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

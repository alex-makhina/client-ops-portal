using Autofac;
using Autofac.Extensions.DependencyInjection;
using ClientOpsPortal.Services.Reporting.Data;
using ClientOpsPortal.Services.Reporting.Services;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

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

var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
    jwksUrl,
    new OpenIdConnectConfigurationRetriever(),
    new HttpDocumentRetriever { RequireHttps = false });

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
                var config = configurationManager.GetConfigurationAsync(CancellationToken.None).GetAwaiter().GetResult();
                return config.SigningKeys;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumers(Assembly.GetExecutingAssembly());

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMq:Host"] ?? "rabbitmq", h =>
        {
            h.Username(builder.Configuration["RabbitMq:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMq:Password"] ?? "guest");
        });

        cfg.ConfigureEndpoints(context);
    });
});

builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    containerBuilder.RegisterType<ReportsService>()
        .Named<IReportsService>("inner");

    containerBuilder.RegisterDecorator<IReportsService>(
        (c, inner) => new ReportsServiceLoggingDecorator(inner, c.Resolve<ILogger<ReportsServiceLoggingDecorator>>()),
        fromKey: "inner");
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

using ClientOpsPortal.Services.Directory.Data;
using ClientOpsPortal.Services.Directory.Data.Entities;
using ClientOpsPortal.Services.Directory.Data.Interceptors;
using ClientOpsPortal.Services.Directory.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// REST over HTTP/1.1 and gRPC over HTTP/2 (h2c) on dedicated endpoints.
builder.WebHost.ConfigureKestrel(options =>
{
    var restPort = builder.Configuration.GetValue<int?>("Kestrel:RestPort") ?? 8080;
    var grpcPort = builder.Configuration.GetValue<int?>("Kestrel:GrpcPort") ?? 8081;

    options.ListenAnyIP(restPort, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1;
    });

    options.ListenAnyIP(grpcPort, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http2;
    });
});

builder.Logging.AddJsonConsole(options =>
{
    options.JsonWriterOptions = new System.Text.Json.JsonWriterOptions { Indented = true };
    options.TimestampFormat = "yyyy-MM-dd'T'HH:mm:ss.fff'Z'";
    options.UseUtcTimestamp = true;
});

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddHttpContextAccessor();
builder.Services.AddGrpc();

builder.Services.AddScoped<AuditableInterceptor>();
builder.Services.AddDbContext<DirectoryDbContext>((sp, options) =>
{
    var interceptor = sp.GetRequiredService<AuditableInterceptor>();
    options.UseNpgsql(builder.Configuration.GetConnectionString("DirectoryDb"))
        .AddInterceptors(interceptor);
});

builder.Services.AddScoped<GenericRepository<TariffPlan>>();
builder.Services.AddScoped<ServiceRepository>();
builder.Services.AddScoped<DirectoryService>();

var redisConfig = builder.Configuration.GetValue<string>("Redis:Configuration") ?? "localhost:6379";
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConfig;
    options.InstanceName = "directory:";
});

builder.Services.Configure<ServiceCacheOptions>(
    builder.Configuration.GetSection("Cache"));

var jwksUrl = builder.Configuration["Jwt:JwksUrl"]
    ?? "http://localhost:5110/.well-known/jwks";
var issuer = builder.Configuration["Jwt:Issuer"] ?? "http://localhost:5110";
var audience = builder.Configuration["Jwt:Audience"] ?? "ClientOpsPortalClient";

var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
    jwksUrl,
    new JwksConfigurationRetriever(),
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
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DirectoryDbContext>();
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
app.MapGrpcService<ClientOpsPortal.Services.Directory.Grpc.DirectoryCatalogGrpcService>();

app.Run();

sealed class JwksConfigurationRetriever : IConfigurationRetriever<OpenIdConnectConfiguration>
{
    public async Task<OpenIdConnectConfiguration> GetConfigurationAsync(string address, IDocumentRetriever retriever, CancellationToken cancel)
    {
        var json = await retriever.GetDocumentAsync(address, cancel).ConfigureAwait(false);
        var keySet = new JsonWebKeySet(json);
        var configuration = new OpenIdConnectConfiguration { JsonWebKeySet = keySet };
        foreach (var key in keySet.GetSigningKeys())
            configuration.SigningKeys.Add(key);
        return configuration;
    }
}

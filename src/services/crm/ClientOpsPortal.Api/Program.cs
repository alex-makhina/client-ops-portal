using ClientOpsPortal.Api.Services;
using ClientOpsPortal.Application;
using ClientOpsPortal.Application.Interfaces;
using ClientOpsPortal.Application.Services;
using ClientOpsPortal.Domain.Interfaces.Services;
using ClientOpsPortal.Infrastructure;
using ClientOpsPortal.Infrastructure.Clients;
using ClientOpsPortal.Infrastructure.Data;
using ClientOpsPortal.Services.Auth.Client;
using ClientOpsPortal.Services.Directory.Client;
using ClientOpsPortal.Services.Notifications.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEFCore(builder.Configuration);
builder.Services.AddProblemDetails();
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddOpenApi();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins("http://127.0.0.1:5022", "http://localhost:5022", "http://127.0.0.1:62000", "http://localhost:62000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddApplicationServices();
builder.Services.AddEmailSettings(builder.Configuration);

var directoryServiceUrl = builder.Configuration.GetValue<string>("ServicesDirectory:BaseUrl")
    ?? "http://localhost:5100";
builder.Services.AddMemoryCache();
builder.Services.AddDirectoryGrpcClient(directoryServiceUrl);
builder.Services.AddSingleton<IDirectoryCacheService, DirectoryCacheService>();

var authServiceUrl = builder.Configuration.GetValue<string>("AuthService:BaseUrl")
    ?? "http://localhost:5110";
builder.Services.AddAuthClient(authServiceUrl);

builder.Services.AddNotificationPublisher(builder.Configuration);

builder.Services.AddHttpClient<ISubscriptionHistoryClient, SubscriptionHistoryClient>(client =>
{
    var baseUrl = builder.Configuration["Services:SubscriptionHistory:BaseUrl"] ?? "http://subscription-history:8080/";
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

var jwksUrl = builder.Configuration["Jwt:JwksUrl"]
    ?? "http://localhost:5110/.well-known/jwks";
var issuer = builder.Configuration["Jwt:Issuer"] ?? "http://localhost:5110";
var audience = builder.Configuration["Jwt:Audience"] ?? "ClientOpsPortalClient";
var jwksClient = new HttpClient();
Task<SecurityKey[]> keysTask = null!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
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

app.EnsureAppDatabase();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.UseDeveloperExceptionPage();
    await app.SeedDatabaseAsync();
}
else
{
    app.UseExceptionHandler();
}
app.UseStatusCodePages();

app.UseCors("AllowBlazorClient");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
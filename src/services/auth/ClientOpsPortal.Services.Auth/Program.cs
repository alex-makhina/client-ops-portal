using ClientOpsPortal.Services.Auth.Data;
using ClientOpsPortal.Services.Auth.Data.Seed;
using ClientOpsPortal.Services.Auth.Domain;
using ClientOpsPortal.Services.Auth.Services;
using ClientOpsPortal.Services.Auth.Settings;
using ClientOpsPortal.Services.Notifications.Client;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;
using Scalar.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

var builder = WebApplication.CreateBuilder(args);

var authSettings = builder.Configuration.GetSection(AuthSettings.SectionName).Get<AuthSettings>() ?? new AuthSettings();
builder.Services.AddSingleton(authSettings);

builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendClients", policy =>
    {
        policy.WithOrigins(authSettings.AllowedOrigins.ToArray())
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var connectionString = builder.Configuration.GetConnectionString("AuthDb")
    ?? "Host=localhost;Port=5432;Database=ClientOpsPortalAuthDb;Username=postgres;Password=dev_pass_123";

builder.Services.AddDbContext<AuthDbContext>(options =>
{
    options.UseNpgsql(connectionString);
    options.UseOpenIddict();
});

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
})
.AddEntityFrameworkStores<AuthDbContext>()
.AddDefaultTokenProviders()
.AddSignInManager();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Login";
    options.LogoutPath = "/Logout";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
});

using var keyLoggerFactory = LoggerFactory.Create(logging => logging.AddConsole());
var rsaKeyProvider = new RsaKeyProvider(
    builder.Environment,
    builder.Configuration,
    keyLoggerFactory.CreateLogger<RsaKeyProvider>());

builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore()
               .UseDbContext<AuthDbContext>();
    })
    .AddServer(options =>
    {
        options.SetIssuer(new Uri(authSettings.Issuer))
               .SetAuthorizationEndpointUris("connect/authorize")
               .SetEndSessionEndpointUris("connect/logout")
               .SetTokenEndpointUris("connect/token")
               .SetUserInfoEndpointUris("connect/userinfo");

        options.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.Roles, "api");

        options.AllowAuthorizationCodeFlow()
               .AllowRefreshTokenFlow();

        options.AddSigningKey(rsaKeyProvider.SecurityKey)
               .AddEncryptionKey(rsaKeyProvider.SecurityKey);

        options.UseAspNetCore()
               .DisableTransportSecurityRequirement()
               .EnableAuthorizationEndpointPassthrough()
               .EnableEndSessionEndpointPassthrough()
               .EnableTokenEndpointPassthrough()
               .EnableUserInfoEndpointPassthrough();

        options.DisableAccessTokenEncryption();
    })
    .AddValidation(options =>
    {
        options.UseLocalServer();
        options.UseAspNetCore();
    });

builder.Services.AddAuthorization();
builder.Services.AddScoped<AuthDbSeeder>();
builder.Services.AddScoped<OpenIddictClientSeeder>();
builder.Services.AddNotificationPublisher(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

    if (app.Environment.IsDevelopment())
    {
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
    }
    else
    {
        db.Database.EnsureCreated();
    }

    await scope.ServiceProvider.GetRequiredService<AuthDbSeeder>().SeedAsync();
    await scope.ServiceProvider.GetRequiredService<OpenIddictClientSeeder>().SeedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("FrontendClients");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();

app.Run();

using ClientOpsPortal.Api.Services;
using ClientOpsPortal.Application.Interfaces;
using ClientOpsPortal.Application.Services;
using ClientOpsPortal.Domain.Interfaces.Services;
using ClientOpsPortal.Infrastructure;
using ClientOpsPortal.Infrastructure.Data;
using ClientOpsPortal.Application;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ClientOpsPortal.Application.Settings;
using ClientOpsPortal.Services.Directory.Client;
using ClientOpsPortal.Services.Auth.Client;

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
builder.Services.AddServicesDirectoryClient(directoryServiceUrl);
builder.Services.AddSingleton<IDirectoryCacheService, DirectoryCacheService>();

var authServiceUrl = builder.Configuration.GetValue<string>("AuthService:BaseUrl")
    ?? "http://localhost:5110";
builder.Services.AddAuthClient(authServiceUrl);

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

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
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]))
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
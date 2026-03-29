using ClientOpsPortal.Api.Services;
using ClientOpsPortal.Domain.Entities;
using ClientOpsPortal.Domain.Interfaces.Services;
using ClientOpsPortal.Infrastructure;
using ClientOpsPortal.Infrastructure.Data;
using ClientOpsPortal.Infrastructure.Data.Context;
using Microsoft.AspNetCore.Identity;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEFCore(builder.Configuration);
builder.Services.AddProblemDetails();
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddScoped<ICurrentUserService,CurrentUserService>();

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddDefaultTokenProviders();

var app = builder.Build();

app.MigrateAppAuthDatabase();
app.MigrateAppDatabase();

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

app.MapControllers();

app.Run();

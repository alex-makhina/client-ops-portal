using ClientOpsPortal.Services.Directory.Contracts.Models;
using ClientOpsPortal.Services.Directory.Data;
using ClientOpsPortal.Services.Directory.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();

builder.Services.AddDbContext<DirectoryDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DirectoryDb")));

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

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DirectoryDbContext>();
    db.Database.EnsureCreated();
}

app.UseExceptionHandler();
app.UseStatusCodePages();
app.MapControllers();

app.Run();

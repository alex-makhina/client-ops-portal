using ClientOpsPortal.Services.Reporting.Data;
using ClientOpsPortal.Services.Reporting.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();

builder.Services.AddDbContext<ReportsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ReportingDb")));

builder.Services.AddScoped<ReportsRepository>();
builder.Services.AddScoped<ReportsService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ReportsDbContext>();
    db.Database.EnsureCreated();
}

app.UseExceptionHandler();
app.UseStatusCodePages();
app.MapControllers();

app.Run();

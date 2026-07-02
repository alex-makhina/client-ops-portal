
using AddressValidator.Domain.Repositories;
using AddressValidator.Infrastructure.Persistence;
using AddressValidator.Infrastructure.Repositories;
using Elastic.Clients.Elasticsearch;
using Microsoft.EntityFrameworkCore;

// =====================================================================
// Program.cs — точка входа ASP.NET Core Web API (Фаза 1: Postgres only).
// =====================================================================

var builder = WebApplication.CreateBuilder(args);

// 1. EF Core DbContext (PostgreSQL) ---------------------------------
var pgConnStr = builder.Configuration.GetConnectionString("Postgres")
                ?? throw new InvalidOperationException("ConnectionStrings:Postgres не задан");

builder.Services.AddDbContext<AddressDbContext>(opt =>
{
    opt.UseNpgsql(pgConnStr, npgsql => npgsql.UseNetTopologySuite());
    if (builder.Environment.IsDevelopment())
    {
        opt.EnableSensitiveDataLogging().EnableDetailedErrors();
    }
});

// 2. Elasticsearch client --------------------------------------------
builder.Services.Configure<ElasticsearchSettings>(
    builder.Configuration.GetSection("Elasticsearch"));

var esSection = builder.Configuration.GetSection("Elasticsearch");
var esUri = esSection["Uri"] ?? "http://localhost:9200";
var esTimeout = TimeSpan.Parse(esSection["RequestTimeout"] ?? "00:00:30");

var esClient = new ElasticsearchClient(new ElasticsearchClientSettings(new Uri(esUri))
    .RequestTimeout(esTimeout)
    .EnableDebugMode()
    .DefaultMappingFor<AddressDocument>(m => m
        .IndexName(esSection["IndexName"] ?? "addresses")
        .IdProperty(d => d.Id)));
builder.Services.AddSingleton(esClient);


// 3. Domain interfaces → Infrastructure implementations -------------
builder.Services.AddScoped<IAddressObjectRepository, AddressObjectRepository>();
builder.Services.AddScoped<ISearchRepository, SearchRepository>();

// 4. Controllers + Swagger -------------------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title   = "Address Validator API",
        Version = "v1",
        Description = ""
    });
});

builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AddressDbContext>();
    try
    {
        await db.Database.MigrateAsync();
        app.Logger.LogInformation("EF Core миграции применены успешно.");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Ошибка применения EF Core миграций.");
        throw;  
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Address Validator API v1");
        c.RoutePrefix = "swagger";
    });
}


app.UseCors();
//app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();

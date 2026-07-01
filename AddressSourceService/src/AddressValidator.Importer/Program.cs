using System.Net.Http.Headers;
using System.Text;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Bulk;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AddressValidator.Infrastructure.Persistence;
using AddressValidator.Infrastructure.Repositories;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(o =>
{
    o.SingleLine      = true;
    o.TimestampFormat = "HH:mm:ss ";
});

var config = builder.Configuration;

// 
//var pgConnStr = config.GetConnectionString("Postgres")
//                ?? throw new InvalidOperationException("ConnectionStrings:Postgres не задан");
var pgConnStr = "Host=localhost;Port=5432;Database=addresses;Username=addr;Password=addr_secret;Timeout=15;CommandTimeout=120";

builder.Services.AddDbContext<AddressDbContext>(opt =>
{
    opt.UseNpgsql(pgConnStr, npgsql => npgsql.UseNetTopologySuite());
});

// --- Elasticsearch ---
var esSection   = config.GetSection("Elasticsearch");
var esUri       = esSection["Uri"] ?? "http://localhost:9200";
var esTimeout   = TimeSpan.Parse(esSection["RequestTimeout"] ?? "00:01:00");
var indexName   = esSection["IndexName"] ?? "addresses";
var batchSize   = int.Parse(esSection["BulkBatchSize"] ?? "2000");
var mappingFile = config["MappingFile"] ?? "config/elasticsearch/address-mappings.json";

var esClient = new ElasticsearchClient(new ElasticsearchClientSettings(new Uri(esUri))
    .RequestTimeout(esTimeout)
    .EnableDebugMode()
    .DefaultMappingFor<AddressDocument>(m => m
        .IndexName(indexName)
        .IdProperty(d => d.Id)));
builder.Services.AddSingleton(esClient);
builder.Services.AddSingleton(new IndexerOptions(indexName, batchSize, mappingFile, new Uri(esUri)));
builder.Services.AddHostedService<ImporterHostedService>();

var app = builder.Build();
await app.RunAsync();

// =====================================================================
public sealed record IndexerOptions(string IndexName, int BatchSize, string MappingFile, Uri EsUri);

public sealed class ImporterHostedService : IHostedService
{
    private readonly ElasticsearchClient _es;
    private readonly AddressDbContext _db;
    private readonly IndexerOptions _opts;
    private readonly ILogger<ImporterHostedService> _logger;
    private readonly IHostApplicationLifetime _lifetime;

    public ImporterHostedService(
        ElasticsearchClient es,
        AddressDbContext db,
        IndexerOptions opts,
        ILogger<ImporterHostedService> logger,
        IHostApplicationLifetime lifetime)
    {
        _es = es;
        _db = db;
        _opts = opts;
        _logger = logger;
        _lifetime = lifetime;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await RunAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"FATAL: {ex}");
            Environment.ExitCode = 1;
        }
        _lifetime.StopApplication();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task RunAsync(CancellationToken ct)
    {
        _logger.LogInformation("==> Подготовка индекса {Index} ...", _opts.IndexName);
        await RecreateIndexAsync(ct);

        _logger.LogInformation("==> Чтение адресов из PostgreSQL через EF Core (batch={Batch}) ...", _opts.BatchSize);

        long total = 0, errors = 0;
        var batch = new List<AddressDocument>(_opts.BatchSize);

        await foreach (var addr in _db.AddressObjects
                                      .AsNoTracking()
                                      .OrderBy(a => a.Id)
                                      .AsAsyncEnumerable()
                                      .WithCancellation(ct))
        {
            batch.Add(MapToDocument(addr));

            if (batch.Count >= _opts.BatchSize)
            {
                var (pushed, errs) = await PushBatchAsync(batch, ct);
                total += pushed;
                errors += errs;
                batch.Clear();
                _logger.LogInformation("    -> проиндексировано {Total} адресов", total);
            }
        }

        if (batch.Count > 0)
        {
            var (pushed, errs) = await PushBatchAsync(batch, ct);
            total += pushed;
            errors += errs;
        }

        _logger.LogInformation("==> Готово. Проиндексировано: {Total}, ошибок: {Errors}", total, errors);

        await _es.Indices.RefreshAsync(_opts.IndexName, ct);
    }

    private async Task RecreateIndexAsync(CancellationToken ct)
    {
        using var http = new HttpClient { BaseAddress = _opts.EsUri };

        var delResp = await http.DeleteAsync($"/{_opts.IndexName}", ct);
        if (!delResp.IsSuccessStatusCode && delResp.StatusCode != System.Net.HttpStatusCode.NotFound)
        {
            var body = await delResp.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException($"DELETE index failed: {delResp.StatusCode}\n{body}");
        }

        if (!File.Exists(_opts.MappingFile))
            throw new FileNotFoundException($"Файл mapping не найден: {_opts.MappingFile}");
        var mappingJson = await File.ReadAllTextAsync(_opts.MappingFile, ct);

        using var content = new StringContent(mappingJson, Encoding.UTF8);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        var putResp = await http.PutAsync($"/{_opts.IndexName}", content, ct);
        if (!putResp.IsSuccessStatusCode)
        {
            var body = await putResp.Content.ReadAsStringAsync(ct);
            throw new InvalidOperationException($"PUT index failed: {putResp.StatusCode}\n{body}");
        }

        _logger.LogInformation("    Индекс {Index} создан.", _opts.IndexName);
    }

    private async Task<(long pushed, long errors)> PushBatchAsync(
        List<AddressDocument> batch, CancellationToken ct)
    {
        var bulkRequest = new BulkRequest(_opts.IndexName)
        {
            Operations = new BulkOperationsCollection()
        };

        foreach (var doc in batch)
        {
            bulkRequest.Operations.Add(
                new BulkIndexOperation<AddressDocument>(doc)
                {
                    Id = doc.Id
                });
        }

        var bulkResp = await _es.BulkAsync(bulkRequest, ct);

        long errs = 0;
        if (bulkResp.Errors)
        {
            var errorItems = bulkResp.ItemsWithErrors?
                .Where(i => i is not null)
                .Select(i => new { i.Id, i.Error?.Reason })
                .ToList()
                ?? new();

            if (errorItems.Count > 0)
            {
                foreach (var item in errorItems)
                {
                    errs++;
                    _logger.LogWarning("ES bulk error id={Id}: {Error}", item.Id, item.Reason);
                }
            }
            else
            {
                errs = batch.Count;
                _logger.LogWarning("ES bulk завершился с Errors=true, но без деталей. Весь батч ({Count}) считается неудачным.", batch.Count);
            }
        }
        return (batch.Count - errs, errs);
    }

    private static AddressDocument MapToDocument(AddressValidator.Domain.Entities.AddressObject a)
    {
        return new AddressDocument
        {
            Id       = a.Id.ToString(),
            ParentId = a.ParentId?.ToString(),
            OsmId    = a.OsmId,
            Type     = a.Type.ToString().ToLowerInvariant(),
            Name     = a.Name,
            FullPath = a.FullPath,
        };
    }
}

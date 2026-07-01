using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AddressValidator.Domain.Repositories;

namespace AddressValidator.Infrastructure.Repositories;


public sealed class SearchRepository : ISearchRepository
{
    private readonly ElasticsearchClient _es;
    private readonly IOptions<ElasticsearchSettings> _opts;
    private readonly ILogger<SearchRepository> _logger;

    public SearchRepository(
        ElasticsearchClient es,
        IOptions<ElasticsearchSettings> opts,
        ILogger<SearchRepository> logger)
    {
        _es = es;
        _opts = opts;
        _logger = logger;
    }

    public async Task<List<SearchHit>> SuggestAsync(
        string query, int limit = 10, CancellationToken ct = default)
    {
        query = (query ?? "").Trim();
        if (query.Length < 2) return new();

        limit = Math.Clamp(limit, 1, 50);
        var indexName = _opts.Value.IndexName;

        var resp = await _es.SearchAsync<AddressDocument>(s => s
            .Index(indexName)
            .From(0)
            .Size(limit)
            .Query(q => q.MultiMatch(new MultiMatchQuery
            {
                Query = query,
                Fields = new Field[]
                {
                    "fullPath.prefix",   
                    "name.prefix",       
                    "fullPath",          
                    "name"
                }
            })), ct);

        if (!resp.IsValidResponse)
        {
            _logger.LogError("ES suggest error: {Error}", resp.DebugInformation);
            return new();
        }

        return resp.Hits
            .Where(h => h.Source is not null)
            .Select(h => new SearchHit(
                Id: Guid.TryParse(h.Source!.Id, out var guid) ? guid : Guid.Empty,
                FullPath: h.Source.FullPath ?? "",
                Type: h.Source.Type ?? "unknown",
                Score: h.Score))
            .ToList();
    }

    public async Task<bool> PingAsync(CancellationToken ct = default)
    {
        try { return (await _es.PingAsync(ct)).IsValidResponse; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ES ping failed");
            return false;
        }
    }
}

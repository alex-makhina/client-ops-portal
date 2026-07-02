using Microsoft.AspNetCore.Mvc;
using AddressValidator.Api.Models;
using AddressValidator.Domain.Repositories;
using AddressValidator.Api.Mapper;

namespace AddressValidator.Api.Controllers;

/// <summary>
/// REST API для адресов.
///
/// Эндпоинты:
///   GET /api/addresses/{id:guid}          — получить адрес по UUID (Postgres)
///   GET /api/addresses/{id:guid}/children — дочерние объекты (Postgres)
///   GET /api/addresses/suggest            — автодополнение по строке (Elasticsearch)
///   GET /api/addresses/health             — health check (Postgres + Elasticsearch)
/// </summary>
[ApiController]
[Route("api/addresses")]
[Produces("application/json")]
public sealed class AddressesController : ControllerBase
{
    private readonly IAddressObjectRepository _repo;
    private readonly ISearchRepository _search;
    private readonly ILogger<AddressesController> _logger;

    public AddressesController(
        IAddressObjectRepository repo,
        ISearchRepository search,
        ILogger<AddressesController> logger)
    {
        _repo = repo;
        _search = search;
        _logger = logger;
    }

    /// <summary>Получить адрес по UUID (для других микросервисов).</summary>
    /// <param name="id">UUID адресного объекта (генерируется из osm_id через uuid_v5)</param>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AddressDto>> GetById(Guid id, CancellationToken ct)
    {
        try
        {
            var obj = await _repo.GetByIdAsync(id, ct);
            if (obj is null) return NotFound();
            return Ok(Mapper.Mapper.MapDto(obj));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetById({Id}) failed", id);
            return Problem(detail: "Сервис БД недоступен.", statusCode: 503);
        }
    }

    /// <summary>Получить дочерние объекты (улицы города, здания улицы).</summary>
    [HttpGet("{id:guid}/children")]
    public async Task<ActionResult<List<AddressDto>>> GetChildren(Guid id, CancellationToken ct)
    {
        try
        {
            var children = await _repo.GetChildrenAsync(id, ct);
            return Ok(children.Select(Mapper.Mapper.MapDto).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetChildren({Id}) failed", id);
            return Problem(detail: "Сервис БД недоступен.", statusCode: 503);
        }
    }

    /// <summary>
    /// Автодополнение (typeahead). Возвращает варианты адресов,
    /// отсортированные по релевантности.
    ///
    /// Пример: GET /api/addresses/suggest?query=минск+незави&amp;limit=10
    /// </summary>
    /// <param name="query">Строка запроса (минимум 2 символа)</param>
    /// <param name="limit">Максимум вариантов (по умолчанию 10, максимум 50)</param>
    [HttpGet("suggest")]
    public async Task<ActionResult<List<AddressSuggestion>>> Suggest(
        [FromQuery] string query, [FromQuery] int? limit, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest(new { error = "Query-параметр 'query' обязателен." });

        try
        {
            var hits = await _search.SuggestAsync(query, limit ?? 10, ct);
            return Ok(hits.Select(Mapper.Mapper.MapHit).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Suggest failed: {Query}", query);
            return Problem(detail: ex.Message, statusCode: 503);
        }
    }

    /// <summary>Health check (Postgres + Elasticsearch).</summary>
    [HttpGet("health")]
    public async Task<IActionResult> Health(CancellationToken ct)
    {
        var pgOk = await _repo.PingAsync(ct);
        var esOk = await _search.PingAsync(ct);
        var ok = pgOk && esOk;
        return Ok(new
        {
            status = ok ? "Healthy" : "Degraded",
            postgres = pgOk ? "up" : "down",
            elasticsearch = esOk ? "up" : "down",
            timestamp = DateTime.UtcNow
        });
    }
}

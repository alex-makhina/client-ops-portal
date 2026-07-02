using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AddressValidator.Domain.Entities;
using AddressValidator.Domain.Repositories;
using AddressValidator.Infrastructure.Persistence;

namespace AddressValidator.Infrastructure.Repositories;

public sealed class AddressObjectRepository : IAddressObjectRepository
{
    private readonly AddressDbContext _db;
    private readonly ILogger<AddressObjectRepository> _logger;

    public AddressObjectRepository(
        AddressDbContext db,
        ILogger<AddressObjectRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<bool> PingAsync(CancellationToken ct = default)
    {
        try { return await _db.PingAsync(ct); }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Postgres ping failed");
            return false;
        }
    }

    public async Task<AddressObject?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            return await _db.AddressObjects
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetByIdAsync({Id}) failed", id);
            throw;
        }
    }

    public async Task<List<AddressObject>> GetChildrenAsync(Guid parentId, CancellationToken ct = default)
    {
        try
        {
            return await _db.AddressObjects
                .AsNoTracking()
                .Where(a => a.ParentId == parentId)
                .OrderBy(a => a.Name)
                .ToListAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetChildrenAsync({ParentId}) failed", parentId);
            throw;
        }
    }
}

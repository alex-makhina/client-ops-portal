using AddressValidator.Domain.Entities;

namespace AddressValidator.Domain.Repositories;

public interface IAddressObjectRepository
{
    Task<AddressObject?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<AddressObject>> GetChildrenAsync(Guid parentId, CancellationToken ct = default);

    Task<bool> PingAsync(CancellationToken ct = default);
}

public interface ISearchRepository
{
    Task<List<SearchHit>> SuggestAsync(string query, int limit = 10, CancellationToken ct = default);

    Task<bool> PingAsync(CancellationToken ct = default);
}

public sealed record SearchHit(
    Guid Id,
    string FullPath,
    string Type,
    double? Score);

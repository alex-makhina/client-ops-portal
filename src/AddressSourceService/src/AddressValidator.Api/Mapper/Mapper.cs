using AddressValidator.Api.Models;
using AddressValidator.Domain.Repositories;

namespace AddressValidator.Api.Mapper
{
    public static class Mapper
    {
        public static AddressDto MapDto(Domain.Entities.AddressObject a) => new()
        {
            Id = a.Id,
            FullPath = a.FullPath,
            Type = a.Type.ToString().ToLowerInvariant(),
            ParentId = a.ParentId,
            OsmId = a.OsmId
        };

        public static AddressSuggestion MapHit(SearchHit h) => new()
        {
            Id = h.Id,
            FullPath = h.FullPath,
            Type = h.Type,
            Score = h.Score
        };
    }
}

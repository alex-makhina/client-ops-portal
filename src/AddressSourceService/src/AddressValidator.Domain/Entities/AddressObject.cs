using AddressValidator.Domain.Enums;
using NetTopologySuite.Geometries;

namespace AddressValidator.Domain.Entities;

public sealed class AddressObject
{
    public Guid Id { get; set; }

    public Guid? ParentId { get; set; }
    public AddressObject? Parent { get; set; }

    public List<AddressObject> Children { get; set; } = new();

    public long OsmId { get; set; }

    public string OsmType { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public AddressObjectType Type { get; set; }

    public string FullPath { get; set; } = string.Empty;

    public Point? Geom { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

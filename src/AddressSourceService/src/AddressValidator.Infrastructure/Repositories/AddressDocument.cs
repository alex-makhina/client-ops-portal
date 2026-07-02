using System.Text.Json.Serialization;

namespace AddressValidator.Infrastructure.Repositories;

public sealed class ElasticsearchSettings
{
    public string Uri              { get; set; } = "http://localhost:9200";
    public string IndexName        { get; set; } = "addresses";
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(30);
}
public sealed class AddressDocument
{
    [JsonPropertyName("id")]       public string Id { get; set; } = "";
    [JsonPropertyName("osmId")]    public long? OsmId { get; set; }
    [JsonPropertyName("type")]     public string? Type { get; set; }
    [JsonPropertyName("name")]     public string? Name { get; set; }
    [JsonPropertyName("fullPath")] public string? FullPath { get; set; }
    [JsonPropertyName("parentId")] public string? ParentId { get; set; }
}

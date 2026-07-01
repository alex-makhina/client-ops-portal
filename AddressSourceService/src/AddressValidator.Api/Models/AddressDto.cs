using System.Text.Json.Serialization;

namespace AddressValidator.Api.Models;

/// <summary>DTO для GET /api/addresses/{id} — ответ другим микросервисам.</summary>
public sealed class AddressDto
{
    [JsonPropertyName("id")]        public Guid Id { get; set; }
    [JsonPropertyName("fullPath")]  public string? FullPath { get; set; }
    [JsonPropertyName("type")]      public string? Type { get; set; }
    [JsonPropertyName("parentId")]  public Guid? ParentId { get; set; }
    [JsonPropertyName("osmId")]     public long? OsmId { get; set; }
}

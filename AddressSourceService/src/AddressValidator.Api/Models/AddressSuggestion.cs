using System.Text.Json.Serialization;

namespace AddressValidator.Api.Models;

/// <summary>Один найденный вариант автодополнения (минимум полей).</summary>
public sealed class AddressSuggestion
{
    [JsonPropertyName("id")]       public Guid Id { get; set; }
    [JsonPropertyName("fullPath")] public string? FullPath { get; set; }
    [JsonPropertyName("type")]     public string? Type { get; set; }
    [JsonPropertyName("score")]    public double? Score { get; set; }
}

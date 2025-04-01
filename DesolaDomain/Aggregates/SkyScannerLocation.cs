using System.Text.Json.Serialization;

namespace DesolaDomain.Aggregates;

public class SkyScannerLocation
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("displayCode")]
    public string DisplayCode { get; set; }

    [JsonPropertyName("city")]
    public string City { get; set; }

    [JsonPropertyName("country")]
    public string Country { get; set; }

    [JsonPropertyName("parent")]
    public Parent Parent { get; set; }
}
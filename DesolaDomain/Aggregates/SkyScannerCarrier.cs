using System.Text.Json.Serialization;

namespace DesolaDomain.Aggregates;

public class SkyScannerCarrier
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("logoUrl")]
    public string LogoUrl { get; set; }

    [JsonPropertyName("alternateId")]
    public string AlternateId { get; set; }
}
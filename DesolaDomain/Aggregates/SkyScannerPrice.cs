using System.Text.Json.Serialization;

namespace DesolaDomain.Aggregates;

public class SkyScannerPrice
{
    [JsonPropertyName("raw")]
    public decimal Raw { get; set; }

    [JsonPropertyName("formatted")]
    public string Formatted { get; set; }
}
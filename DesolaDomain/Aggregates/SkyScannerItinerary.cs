using System.Text.Json.Serialization;

namespace DesolaDomain.Aggregates;

public class SkyScannerItinerary
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("price")]
    public SkyScannerPrice Price { get; set; }

    [JsonPropertyName("legs")]
    public List<SkyScannerLeg> Legs { get; set; }
}
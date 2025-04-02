using System.Text.Json.Serialization;

namespace DesolaDomain.Aggregates;

public class SkyScannerData
{
    [JsonPropertyName("itineraries")]
    public List<SkyScannerItinerary> Itineraries { get; set; }
}
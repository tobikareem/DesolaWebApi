using System.Text.Json.Serialization;
using DesolaDomain.Entities.AmadeusFields;

namespace DesolaDomain.Aggregates;

public class SkyScannerLeg
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("origin")]
    public SkyScannerLocation Origin { get; set; }

    [JsonPropertyName("destination")]
    public SkyScannerLocation Destination { get; set; }

    [JsonPropertyName("durationInMinutes")]
    public int DurationInMinutes { get; set; }

    public int StopCount { get; set; }

    [JsonPropertyName("departure")]
    public DateTime Departure { get; set; }

    [JsonPropertyName("arrival")]
    public DateTime Arrival { get; set; }

    [JsonPropertyName("carriers")]
    public SkyScannerCarriers Carriers { get; set; }

    [JsonPropertyName("segments")]
    public List<SkySegment> Segments { get; set; }
}
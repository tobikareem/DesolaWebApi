using System.Text.Json.Serialization;

namespace DesolaDomain.Aggregates;

public class SkyScannerFlightOffer
{
    [JsonPropertyName("data")]
    public SkyScannerData Data { get; set; }
}
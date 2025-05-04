using System.Text.Json.Serialization;

namespace DesolaDomain.Aggregates;

public class SkySegment
{
    [JsonPropertyName("origin")]
    public SkyScannerLocation Origin { get; set; }

    [JsonPropertyName("destination")]
    public SkyScannerLocation Destination { get; set; }

    [JsonPropertyName("marketingCarrier")]
    public SkyScannerCarrier MarketingCarrier{ get; set; }


    [JsonPropertyName("durationInMinutes")]
    public int DurationInMinutes { get; set; }

    [JsonPropertyName("flightNumber")]
    public string FlightNumber { get; set; }

    [JsonPropertyName("departure")]
    public DateTime Departure { get; set; }

    [JsonPropertyName("arrival")]
    public DateTime Arrival { get; set; }


    [JsonPropertyName("operatingCarrier")]
    public SkyScannerCarrier OperatingCarrier { get; set; }
}
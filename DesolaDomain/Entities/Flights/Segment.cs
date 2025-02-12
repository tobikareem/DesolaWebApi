using System.Text.Json.Serialization;
using DesolaDomain.Aggregates;

namespace DesolaDomain.Entities.Flights;

public class Segment
{
    [JsonPropertyName("departure")]
    public Arrival Departure { get; set; }

    [JsonPropertyName("arrival")]
    public Arrival Arrival { get; set; }

    [JsonPropertyName("carrierCode")]
    public string CarrierCode { get; set; }

    [JsonPropertyName("number")]
    public string Number { get; set; }

    [JsonPropertyName("aircraft")]
    public Aircraft Aircraft { get; set; }

    [JsonPropertyName("operating")]
    public Operating Operating { get; set; }

    [JsonPropertyName("duration")]
    public string Duration { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("numberOfStops")]
    public int NumberOfStops { get; set; }

    [JsonPropertyName("blacklistedInEU")]
    public bool BlacklistedInEu { get; set; }


}

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
using System.Text.Json.Serialization;

namespace DesolaDomain.Aggregates;

public class Parent
{
    [JsonPropertyName("flightPlaceId")]
    public string FlightPlaceId { get; set; }
    [JsonPropertyName("displayCode")]
    public string DisplayCode { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("type")]
    public string Type { get; set; }
}
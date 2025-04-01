using System.Text.Json.Serialization;
using DesolaDomain.Entities.Flights;

namespace DesolaDomain.Aggregates;

public class RouteLocation
{
    [JsonPropertyName("type")]
    public string Type;

    [JsonPropertyName("subtype")]
    public string Subtype;

    [JsonPropertyName("name")]
    public string Name;

    [JsonPropertyName("iataCode")]
    public string IataCode;

    [JsonPropertyName("geoCode")]
    public GeoCode GeoCode;

    [JsonPropertyName("address")]
    public Address Address;

    [JsonPropertyName("timeZone")]
    public RouteTimeZone TimeZone;
}
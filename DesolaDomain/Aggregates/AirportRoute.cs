using DesolaDomain.Entities.Flights;
using System.Text.Json.Serialization;
namespace DesolaDomain.Aggregates;

public class AirportRoute
{
    [JsonPropertyName("meta")]
    public Meta Meta { get; set; }

    [JsonPropertyName("data")]
    public List<RouteLocation> Data { get; set; }
}

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

public class GeoCode
{
    [JsonPropertyName("latitude")]
    public double Latitude;

    [JsonPropertyName("longitude")]
    public double Longitude;
}
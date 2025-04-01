using System.Text.Json.Serialization;

namespace DesolaDomain.Aggregates;

public class GeoCode
{
    [JsonPropertyName("latitude")]
    public double Latitude;

    [JsonPropertyName("longitude")]
    public double Longitude;
}
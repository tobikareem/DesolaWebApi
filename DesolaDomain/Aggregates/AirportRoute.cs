using System.Text.Json.Serialization;
using amadeus.resources;

namespace DesolaDomain.Aggregates;

public class AirportRoute
{
    [JsonPropertyName("meta")]
    public Meta Meta { get; set; }

    [JsonPropertyName("data")]
    public List<dynamic> Data { get; set; }
}
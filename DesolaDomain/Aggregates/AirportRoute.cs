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
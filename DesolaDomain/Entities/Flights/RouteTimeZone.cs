
using System.Text.Json.Serialization;

namespace DesolaDomain.Entities.Flights;

public class RouteTimeZone
{

    [JsonPropertyName("offSet")]
    public string OffSet;

    [JsonPropertyName("referenceLocalDateTime")]
    public DateTime ReferenceLocalDateTime;
}
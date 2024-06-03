using System.Text.Json.Serialization;

namespace DesolaDomain.Entities.Flights;

public class Carriers
{
    [JsonPropertyName("6X")]
    public string The6X { get; set; }
}
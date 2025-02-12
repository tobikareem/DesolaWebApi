using System.Text.Json.Serialization;

namespace DesolaDomain.Entities.Flights;

public class Dictionaries
{
    [JsonPropertyName("locations")]
    public Dictionary<string, Location> Locations { get; set; }

    [JsonPropertyName("aircraft")]
    public Dictionary<string, string> Aircraft { get; set; }

    [JsonPropertyName("currencies")]
    public Currencies Currencies { get; set; }

    [JsonPropertyName("carriers")]
    public Carriers Carriers { get; set; }
}